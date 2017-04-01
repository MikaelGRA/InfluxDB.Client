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
         foreach( var fieldOrTag in columns )
         {
            if( fieldOrTag != InfluxConstants.KeyColumn && fieldOrTag != InfluxConstants.TimeColumn && !meta.Tags.Contains( fieldOrTag ) && !meta.Fields.Contains( fieldOrTag ) )
            {
               return false;
            }
         }
         return true;
      }

      internal static InfluxResultSet Create( QueryResult queryResult )
      {
         List<InfluxResult> results = new List<InfluxResult>();
         foreach( var result in queryResult.Results )
         {
            results.Add( new InfluxResult( result.StatementId, result.Error ) );
         }
         return new InfluxResultSet( results );
      }

      internal static InfluxResultSet Create( IEnumerable<QueryResult> queryResults )
      {
         Dictionary<int, InfluxResult> results = new Dictionary<int, InfluxResult>();
         foreach( var queryResult in queryResults )
         {
            foreach( var result in queryResult.Results )
            {
               InfluxResult existingResult;
               if( !results.TryGetValue( result.StatementId, out existingResult ) )
               {
                  results.Add( result.StatementId, new InfluxResult( result.StatementId, result.Error ) );
               }
               else
               {
                  existingResult.AppendErrorMessage( result.Error );
               }
            }
         }
         return new InfluxResultSet( results.Values.ToList() );
      }

      internal static Task<InfluxResultSet<TInfluxRow>> CreateAsync<TInfluxRow>(
         InfluxClient client,
         IEnumerable<QueryResult> queryResult,
         string db,
         TimestampPrecision? precision,
         bool isExclusivelyFields )
         where TInfluxRow : new()
      {
         if( IsIInfluxRow<TInfluxRow>() )
         {
            return CreateBasedOnInterfaceAsync<TInfluxRow>( client, queryResult, db, precision, isExclusivelyFields );
         }
         else
         {
            return Task.FromResult( CreateBasedOnAttributes<TInfluxRow>( queryResult, precision ) );
         }
      }

      private static InfluxResultSet<TInfluxRow> CreateBasedOnAttributes<TInfluxRow>(
         IEnumerable<QueryResult> queryResults,
         TimestampPrecision? precision )
         where TInfluxRow : new()
      {
         // Create type based on attributes
         Dictionary<int, InfluxResult<TInfluxRow>> results = new Dictionary<int, InfluxResult<TInfluxRow>>();
         var propertyMap = MetadataCache.GetOrCreate<TInfluxRow>().All;

         foreach( var queryResult in queryResults )
         {
            foreach( var result in queryResult.Results )
            {
               InfluxResult<TInfluxRow> existingResult;
               if( !results.TryGetValue( result.StatementId, out existingResult ) )
               {
                  existingResult = new InfluxResult<TInfluxRow>( result.StatementId, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) );
                  results.Add( result.StatementId, existingResult );
               }
               else
               {
                  existingResult.AppendErrorMessage( result.Error );
               }

               // TODO: What if error message is given in following query?
               if( existingResult.Succeeded )
               {
                  foreach( var series in result.Series )
                  {
                     var name = series.Name;
                     var columns = series.Columns;
                     var dataPoints = new List<TInfluxRow>();

                     // create new dictionary, with correct typing (we must potentially convert strings to enums, if the column being used in the GROUP BY is an enum)
                     Dictionary<string, object> tags = null;
                     if( series.Tags != null )
                     {
                        tags = new Dictionary<string, object>();
                        foreach( var kvp in series.Tags )
                        {
                           object value;
                           if( !string.IsNullOrEmpty( kvp.Value ) )
                           {
                              PropertyExpressionInfo<TInfluxRow> property;
                              if( propertyMap.TryGetValue( kvp.Key, out property ) )
                              {
                                 // we know this is either an enum or a string
                                 if( property.IsEnum )
                                 {
                                    Enum valueAsEnum;
                                    if( property.StringToEnum.TryGetValue( kvp.Value, out valueAsEnum ) )
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
                     var influxSerie = existingResult.FindGroupInternal( name, tags, true );
                     if( influxSerie == null )
                     {
                        influxSerie = new InfluxSeries<TInfluxRow>( name, tags );
                        existingResult.AddInfluxSeries( influxSerie );
                     }

                     // Values will be null, if there are no entries in the result set
                     if( series.Values != null )
                     {
                        // construct an array of properties based on the same indexing as the columns returned by the query
                        var properties = new PropertyExpressionInfo<TInfluxRow>[ columns.Count ];
                        for( int i = 0 ; i < columns.Count ; i++ )
                        {
                           PropertyExpressionInfo<TInfluxRow> propertyInfo;
                           if( propertyMap.TryGetValue( columns[ i ], out propertyInfo ) )
                           {
                              properties[ i ] = propertyInfo;
                           }
                        }

                        foreach( var values in series.Values )
                        {
                           // construct the data points based on the attributes
                           var row = new TInfluxRow();

                           // if we implement IHaveMeasurementName, set the measurement name on the IInfluxRow as well
                           var seriesDataPoint = dataPoints as IHaveMeasurementName;
                           if( seriesDataPoint != null )
                           {
                              seriesDataPoint.MeasurementName = name;
                           }

                           for( int i = 0 ; i < values.Count ; i++ )
                           {
                              var value = values[ i ]; // TODO: What about NULL values? Are they treated as empty strings or actual nulls?
                              var property = properties[ i ];

                              // set the value based on the property, if both the value and property is not null
                              if( property != null )
                              {
                                 if( value != null )
                                 {
                                    if( property.IsDateTime )
                                    {
                                       if( value is string )
                                       {
                                          property.SetValue( row, DateTime.Parse( (string)value, CultureInfo.InvariantCulture, DateTimeStyles ) );
                                       }
                                       else if( value is long )
                                       {
                                          property.SetValue( row, DateTimeExtensions.FromEpochTime( (long)value, precision.Value ) );
                                       }
                                    }
                                    else if( property.IsEnum )
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

                     influxSerie.AddRows( dataPoints );
                  }
               }
            }
         }

         return new InfluxResultSet<TInfluxRow>( results.Values.ToList() );
      }

      private async static Task<InfluxResultSet<TInfluxRow>> CreateBasedOnInterfaceAsync<TInfluxRow>(
         InfluxClient client,
         IEnumerable<QueryResult> queryResults,
         string db,
         TimestampPrecision? precision,
         bool isExclusivelyFields )
         where TInfluxRow : new()
      {
         // In this case, we will contruct objects based on the IInfluxRow interface
         Dictionary<int, InfluxResult<TInfluxRow>> results = new Dictionary<int, InfluxResult<TInfluxRow>>();
         foreach( var queryResult in queryResults )
         {
            foreach( var result in queryResult.Results )
            {
               InfluxResult<TInfluxRow> existingResult;
               if( !results.TryGetValue( result.StatementId, out existingResult ) )
               {
                  existingResult = new InfluxResult<TInfluxRow>( result.StatementId, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) );
                  results.Add( result.StatementId, existingResult );
               }
               else
               {
                  existingResult.AppendErrorMessage( result.Error );
               }

               if( existingResult.Succeeded )
               {
                  foreach( var series in result.Series )
                  {
                     var name = series.Name;
                     var columns = series.Columns;
                     var dataPoints = new List<TInfluxRow>();
                     var setters = new Action<IInfluxRow, string, object>[ columns.Count ];

                     // create influx series
                     var tags = series.Tags?.ToDictionary( x => x.Key, x => x.Value == string.Empty ? null : (object)x.Value ) ?? null;
                     var influxSerie = existingResult.FindGroupInternal( name, tags, true );
                     if( influxSerie == null )
                     {
                        influxSerie = new InfluxSeries<TInfluxRow>( name, tags );
                        existingResult.AddInfluxSeries( influxSerie );
                     }

                     // Values will be null, if there are no entries in the result set
                     if( series.Values != null )
                     {
                        // Get metadata information about the measurement we are querying, as we dont know
                        // which columns are tags/fields otherwise
                        DatabaseMeasurementInfo meta = null;
                        if( !isExclusivelyFields )
                        {
                           // get the required metadata
                           meta = await client.GetMetaInformationAsync( db, name, false ).ConfigureAwait( false );

                           // check that we have all columns, otherwise call method again
                           bool hasAllColumnsAndTags = HasAllColumns( meta, columns );
                           if( !hasAllColumnsAndTags )
                           {
                              // if we dont have all columns, attempt to query the metadata again (might have changed since last query)
                              meta = await client.GetMetaInformationAsync( db, name, false ).ConfigureAwait( false );
                              hasAllColumnsAndTags = HasAllColumns( meta, columns );

                              // if we still dont have all columns, we cant do anything, throw exception
                              if( !hasAllColumnsAndTags )
                              {
                                 throw new InfluxException( Errors.IndeterminateColumns );
                              }
                           }
                        }

                        for( int i = 0 ; i < columns.Count ; i++ )
                        {
                           var columnName = columns[ i ];

                           if( isExclusivelyFields )
                           {
                              setters[ i ] = ( row, fieldName, value ) => row.SetField( fieldName, value );
                           }
                           else if( columnName == InfluxConstants.TimeColumn )
                           {
                              setters[ i ] = ( row, timeName, value ) =>
                              {
                                 if( value is string )
                                 {
                                    row.SetTimestamp( DateTime.Parse( (string)value, CultureInfo.InvariantCulture, DateTimeStyles ) );
                                 }
                                 else if( value is long )
                                 {
                                    row.SetTimestamp( DateTimeExtensions.FromEpochTime( (long)value, precision.Value ) );
                                 }
                              };
                           }
                           else if( meta.Tags.Contains( columnName ) )
                           {
                              setters[ i ] = ( row, tagName, value ) => row.SetTag( tagName, (string)value );
                           }
                           else if( meta.Fields.Contains( columnName ) )
                           {
                              setters[ i ] = ( row, fieldName, value ) => row.SetField( fieldName, value );
                           }
                           else
                           {
                              throw new InfluxException( string.Format( Errors.InvalidColumn, columnName ) );
                           }
                        }

                        // constructs the IInfluxRows using the IInfluxRow interface
                        foreach( var values in series.Values )
                        {
                           var dataPoint = (IInfluxRow)new TInfluxRow();

                           // if we implement IHaveMeasurementName, set the measurement name on the IInfluxRow as well
                           var seriesDataPoint = dataPoints as IHaveMeasurementName;
                           if( seriesDataPoint != null )
                           {
                              seriesDataPoint.MeasurementName = name;
                           }

                           // go through all values that are stored as a List<List<object>>
                           for( int i = 0 ; i < values.Count ; i++ )
                           {
                              var value = values[ i ]; // TODO: What about NULL values? Are they treated as empty strings or actual nulls?
                              if( value != null )
                              {
                                 setters[ i ]( dataPoint, columns[ i ], value );
                              }
                           }

                           dataPoints.Add( (TInfluxRow)dataPoint );
                        }
                     }

                     influxSerie.AddRows( dataPoints );
                  }
               }
            }
         }

         return new InfluxResultSet<TInfluxRow>( results.Values.ToList() );
      }
   }
}
