using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class ResultSetFactory
   {
      internal static bool IsCustomDataPoint<TInfluxRow>()
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

      internal async static Task<InfluxResultSet<TInfluxRow>> CreateAsync<TInfluxRow>(
         InfluxClient client,
         QueryResult queryResult,
         string db,
         bool isExclusivelyFields )
         where TInfluxRow : new()
      {
         if ( IsCustomDataPoint<TInfluxRow>() )
         {
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

                     if ( series.Values != null )
                     {
                        DatabaseMeasurementInfo meta = null;

                        // get a collection of tags/fields, and ensure all columns exists in it
                        if ( !isExclusivelyFields )
                        {
                           string seriesName = series.Name;
                           meta = await client.GetMetaInformationAsync( db, seriesName, false ).ConfigureAwait( false );

                           //check that we have all columns, otherwise call method again
                           bool hasAllColumnsAndTags = HasAllColumns( meta, columns );
                           if ( !hasAllColumnsAndTags )
                           {
                              meta = await client.GetMetaInformationAsync( db, seriesName, false ).ConfigureAwait( false );
                              hasAllColumnsAndTags = HasAllColumns( meta, columns );

                              if ( !hasAllColumnsAndTags )
                              {
                                 throw new InfluxException( "Could not determine which columns in the returned data is tags/fields." );
                              }
                           }
                        }

                        foreach ( var values in series.Values )
                        {
                           var dataPoint = (IInfluxRow)new TInfluxRow();
                           var seriesDataPoint = dataPoints as IHaveMeasurementName;
                           if ( seriesDataPoint != null )
                           {
                              seriesDataPoint.MeasurementName = name;
                           }

                           for ( int i = 0 ; i < values.Count ; i++ )
                           {
                              if ( isExclusivelyFields )
                              {
                                 dataPoint.SetField( columns[ i ], values[ i ] );
                              }
                              else
                              {
                                 // Is this a field or a tag?
                                 var columnName = columns[ i ];
                                 var value = values[ i ];
                                 if ( value != null )
                                 {
                                    if ( columnName == InfluxConstants.TimeColumn )
                                    {
                                       dataPoint.SetTimestamp( (DateTime)value );
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
         else
         {
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

                     if ( series.Values != null )
                     {
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
                           var dataPoint = new TInfluxRow();
                           var seriesDataPoint = dataPoints as IHaveMeasurementName;
                           if ( seriesDataPoint != null )
                           {
                              seriesDataPoint.MeasurementName = name;
                           }

                           for ( int i = 0 ; i < values.Count ; i++ )
                           {
                              var value = values[ i ];
                              var property = properties[ i ];
                              if ( property != null )
                              {
                                 if ( value != null )
                                 {
                                    if ( property.Type.IsEnum )
                                    {
                                       property.SetValue( dataPoint, property.GetEnumValue( value ) );
                                    }
                                    else
                                    {
                                       property.SetValue( dataPoint, Convert.ChangeType( value, property.Type ) );
                                    }
                                 }
                                 // TODO: require that it is nullable in an ELSE?
                              }
                           }

                           dataPoints.Add( dataPoint );
                        }
                     }

                     // create new dictionary, with correct typing
                     Dictionary<string, object> tags = null;
                     if ( series.Tags != null )
                     {
                        tags = new Dictionary<string, object>();
                        foreach ( var kvp in series.Tags )
                        {
                           object value;
                           if ( kvp.Value != string.Empty )
                           {
                              PropertyExpressionInfo<TInfluxRow> property;
                              if ( propertyMap.TryGetValue( kvp.Key, out property ) )
                              {
                                 // we know this is either an enum or a string
                                 if ( property.Type.IsEnum )
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
      }
   }
}
