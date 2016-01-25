using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class ResultSetFactory
   {
      private static readonly DateTimeStyles DateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

      internal static bool IsIInfluxRow<TInfluxRow>()
      {
         return typeof( IInfluxRow ).IsAssignableFrom( typeof( TInfluxRow ) );
      }

      private static bool HasAllColumns( DatabaseMeasurementInfo meta, List<string> columns )
      {
         foreach ( var fieldOrTag in columns )
         {
            if ( fieldOrTag != InfluxConstants.KeyColumn && fieldOrTag != InfluxConstants.TimeColumn && !meta.Tags.Contains( fieldOrTag ) && !meta.Fields.Contains( fieldOrTag ) )
            {
               return false;
            }
         }
         return true;
      }

      internal static InfluxResultSet Create( QueryResult queryResult )
      {
         List<InfluxResult> results = new List<InfluxResult>();
         foreach ( var result in queryResult.Results )
         {
            results.Add( new InfluxResult( result.Error ) );
         }
         return new InfluxResultSet( results );
      }

      internal static Task<InfluxResultSet<TInfluxRow>> CreateAsync<TInfluxRow>(
         InfluxClient client,
         QueryResult queryResult,
         string db,
         bool isExclusivelyFields )
         where TInfluxRow : new()
      {
         if ( IsIInfluxRow<TInfluxRow>() )
         {
            return CreateBasedOnInterfaceAsync<TInfluxRow>( client, queryResult, db, isExclusivelyFields );
         }
         else
         {
            return Task.FromResult( CreateBasedOnAttributes<TInfluxRow>( queryResult ) );
         }
      }

      private static InfluxResultSet<TInfluxRow> CreateBasedOnAttributes<TInfluxRow>( QueryResult queryResult )
         where TInfluxRow : new()
      {
         // Create type based on attributes
         List<InfluxResult<TInfluxRow>> results = new List<InfluxResult<TInfluxRow>>();
         var propertyMap = MetadataCache.GetOrCreate<TInfluxRow>().All;

         foreach ( var result in queryResult.Results )
         {
            var influxSeries = new List<InfluxSeries<TInfluxRow>>();
            if ( result.Series != null && result.Error == null )
            {
               foreach ( var series in result.Series )
               {
                  var name = series.Name;
                  var columns = series.Columns;
                  var dataPoints = new List<TInfluxRow>();

                  // Values will be null, if there are no entries in the result set
                  if ( series.Values != null )
                  {
                     // construct an array of properties based on the same indexing as the columns returned by the query
                     var properties = new PropertyExpressionInfo<TInfluxRow>[ columns.Count ];
                     for ( int i = 0 ; i < columns.Count ; i++ )
                     {
                        PropertyExpressionInfo<TInfluxRow> propertyInfo;
                        if ( propertyMap.TryGetValue( columns[ i ], out propertyInfo ) )
                        {
                           properties[ i ] = propertyInfo;
                        }
                     }

                     foreach ( var values in series.Values )
                     {
                        // construct the data points based on the attributes
                        var row = new TInfluxRow();

                        // if we implement IHaveMeasurementName, set the measurement name on the IInfluxRow as well
                        var seriesDataPoint = dataPoints as IHaveMeasurementName;
                        if ( seriesDataPoint != null )
                        {
                           seriesDataPoint.MeasurementName = name;
                        }

                        for ( int i = 0 ; i < values.Count ; i++ )
                        {
                           var value = values[ i ]; // TODO: What about NULL values? Are they treated as empty strings or actual nulls?
                           var property = properties[ i ];

                           // set the value based on the property, if both the value and property is not null
                           if ( property != null )
                           {
                              if ( value != null )
                              {
                                 if ( property.IsDateTime )
                                 {
                                    property.SetValue( row, DateTime.Parse( (string)value, CultureInfo.InvariantCulture, DateTimeStyles ) );
                                 }
                                 else if ( property.IsEnum )
                                 {
                                    property.SetValue( row, property.GetEnumValue( value ) );
                                 }
                                 else
                                 {
                                    property.SetValue( row, Convert.ChangeType( value, property.Type, CultureInfo.InvariantCulture ) );
                                 }
                              }
                           }
                        }

                        dataPoints.Add( row );
                     }
                  }

                  // create new dictionary, with correct typing (we must potentially convert strings to enums, if the column being used in the GROUP BY is an enum)
                  Dictionary<string, object> tags = null;
                  if ( series.Tags != null )
                  {
                     tags = new Dictionary<string, object>();
                     foreach ( var kvp in series.Tags )
                     {
                        object value;
                        if ( !string.IsNullOrEmpty( kvp.Value ) )
                        {
                           PropertyExpressionInfo<TInfluxRow> property;
                           if ( propertyMap.TryGetValue( kvp.Key, out property ) )
                           {
                              // we know this is either an enum or a string
                              if ( property.IsEnum )
                              {
                                 Enum valueAsEnum;
                                 if ( property.StringToEnum.TryGetValue( kvp.Value, out valueAsEnum ) )
                                 {
                                    value = valueAsEnum;
                                 }
                                 else
                                 {
                                    // could not find the value, simply use the string representation
                                    value = kvp.Value;
                                 }
                              }
                              else
                              {
                                 // since kvp.Value is just a string, so go for it
                                 value = kvp.Value;
                              }
                           }
                           else
                           {
                              value = kvp.Value;
                           }
                        }
                        else
                        {
                           value = null;
                        }

                        tags.Add( kvp.Key, value );
                     }
                  }

                  influxSeries.Add( new InfluxSeries<TInfluxRow>( name, dataPoints, tags ) );
               }
            }

            results.Add( new InfluxResult<TInfluxRow>( influxSeries, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) ) );
         }

         return new InfluxResultSet<TInfluxRow>( results );
      }

      private async static Task<InfluxResultSet<TInfluxRow>> CreateBasedOnInterfaceAsync<TInfluxRow>(
         InfluxClient client,
         QueryResult queryResult,
         string db,
         bool isExclusivelyFields )
         where TInfluxRow : new()
      {
         // In this case, we will contruct objects based on the IInfluxRow interface
         List<InfluxResult<TInfluxRow>> results = new List<InfluxResult<TInfluxRow>>();
         foreach ( var result in queryResult.Results )
         {
            var influxSeries = new List<InfluxSeries<TInfluxRow>>();
            if ( result.Series != null && result.Error == null )
            {
               foreach ( var series in result.Series )
               {
                  var name = series.Name;
                  var columns = series.Columns;
                  var dataPoints = new List<TInfluxRow>();

                  // Values will be null, if there are no entries in the result set
                  if ( series.Values != null )
                  {
                     // Get metadata information about the measurement we are querying, as we dont know
                     // which columns are tags/fields otherwise
                     DatabaseMeasurementInfo meta = null;
                     if ( !isExclusivelyFields )
                     {
                        // get the required metadata
                        meta = await client.GetMetaInformationAsync( db, name, false ).ConfigureAwait( false );

                        // check that we have all columns, otherwise call method again
                        bool hasAllColumnsAndTags = HasAllColumns( meta, columns );
                        if ( !hasAllColumnsAndTags )
                        {
                           // if we dont have all columns, attempt to query the metadata again (might have changed since last query)
                           meta = await client.GetMetaInformationAsync( db, name, false ).ConfigureAwait( false );
                           hasAllColumnsAndTags = HasAllColumns( meta, columns );

                           // if we still dont have all columns, we cant do anything, throw exception
                           if ( !hasAllColumnsAndTags )
                           {
                              throw new InfluxException( Errors.IndeterminateColumns );
                           }
                        }
                     }

                     // constructs the IInfluxRows using the IInfluxRow interface
                     foreach ( var values in series.Values )
                     {
                        var dataPoint = (IInfluxRow)new TInfluxRow();

                        // if we implement IHaveMeasurementName, set the measurement name on the IInfluxRow as well
                        var seriesDataPoint = dataPoints as IHaveMeasurementName;
                        if ( seriesDataPoint != null )
                        {
                           seriesDataPoint.MeasurementName = name;
                        }

                        // go through all values that are stored as a List<List<object>>
                        for ( int i = 0 ; i < values.Count ; i++ )
                        {
                           if ( isExclusivelyFields )
                           {
                              dataPoint.SetField( columns[ i ], values[ i ] );
                           }
                           else
                           {
                              var columnName = columns[ i ];
                              var value = values[ i ]; // TODO: What about NULL values? Are they treated as empty strings or actual nulls?

                              // determine which method to call, if the value exists, otherwise, we wont call any method
                              if ( value != null )
                              {
                                 if ( columnName == InfluxConstants.TimeColumn )
                                 {
                                    dataPoint.SetTimestamp( DateTime.Parse( (string)value, CultureInfo.InvariantCulture, DateTimeStyles ) );
                                 }
                                 else if ( meta.Tags.Contains( columnName ) )
                                 {
                                    dataPoint.SetTag( columnName, (string)value );
                                 }
                                 else
                                 {
                                    dataPoint.SetField( columnName, value );
                                 }
                              }
                           }
                        }

                        dataPoints.Add( (TInfluxRow)dataPoint );
                     }
                  }

                  // create new dictionary, with correct typing
                  var tags = series.Tags?.ToDictionary( x => x.Key, x => x.Value == string.Empty ? null : (object)x.Value ) ?? null;

                  influxSeries.Add( new InfluxSeries<TInfluxRow>( name, dataPoints, tags ) );
               }
            }

            results.Add( new InfluxResult<TInfluxRow>( influxSeries, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) ) );
         }

         return new InfluxResultSet<TInfluxRow>( results );
      }
   }
}
