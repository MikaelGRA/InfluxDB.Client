using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Helpers;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class QueryTransform
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

      internal static InfluxResultSet ParseQuery( QueryResult queryResult )
      {
         List<InfluxResult> results = new List<InfluxResult>();
         foreach ( var result in queryResult.Results )
         {
            results.Add( new InfluxResult( result.Error ) );
         }
         return new InfluxResultSet( results );
      }

      internal async static Task<InfluxResultSet<TInfluxRow>> ParseQueryAsync<TInfluxRow>(
         InfluxClient client,
         QueryResult queryResult,
         string db,
         bool isExclusivelyTags )
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
                     DatabaseMeasurementInfo meta = null;

                     // get a collection of tags/fields, and ensure all columns exists in it
                     if ( !isExclusivelyTags )
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

                     var dataPoints = new List<TInfluxRow>();
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
                           if ( isExclusivelyTags )
                           {
                              dataPoint.WriteField( columns[ i ], values[ i ] );
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
                                    dataPoint.WriteTimestamp( (DateTime)value );
                                 }
                                 else if ( meta.Tags.Contains( columnName ) )
                                 {
                                    dataPoint.WriteTag( columnName, (string)value );
                                 }
                                 else
                                 {
                                    dataPoint.WriteField( columnName, value );
                                 }
                              }
                           }
                        }

                        dataPoints.Add( (TInfluxRow)dataPoint );
                     }

                     influxSeries.Add( new InfluxSeries<TInfluxRow>( name, dataPoints, series.Tags ) );
                  }
               }

               results.Add( new InfluxResult<TInfluxRow>( influxSeries, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) ) );
            }

            return new InfluxResultSet<TInfluxRow>( results );
         }
         else
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
                     var properties = new PropertyExpressionInfo<TInfluxRow>[ columns.Count ];
                     var propertyMap = TypeCache.GetOrCreateTypeCache<TInfluxRow>().All;
                     for ( int i = 0 ; i < columns.Count ; i++ )
                     {
                        PropertyExpressionInfo<TInfluxRow> propertyInfo;
                        if ( propertyMap.TryGetValue( columns[ i ], out propertyInfo ) )
                        {
                           properties[ i ] = propertyInfo;
                        }
                     }

                     var dataPoints = new List<TInfluxRow>();
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

                     influxSeries.Add( new InfluxSeries<TInfluxRow>( name, dataPoints, series.Tags ) );
                  }
               }

               results.Add( new InfluxResult<TInfluxRow>( influxSeries, result.Error ?? ( result.Series == null ? Errors.UnexpectedQueryResult : null ) ) );
            }

            return new InfluxResultSet<TInfluxRow>( results );
         }
      }





      //internal static InfluxQueryResultSet<TInfluxRow> ParseQuery<TInfluxRow>( JsonTextReader reader )
      //   where TInfluxRow : IDataPoint, new()
      //{
      //   List<InfluxQueryResult<TInfluxRow>> results = new List<InfluxQueryResult<TInfluxRow>>();

      //   reader.Read(); // Consume None, go to StartObject
      //   reader.Read(); // Consume StartObject, go to PropertyName
      //   reader.Read(); // Consume PropertyName ("results"), go to StartArray
      //   while ( reader.Read() && reader.TokenType != JsonToken.EndObject ) // Consnume StartArray/EndObject, go to StartObject/EndObject
      //   {

      //      while ( reader.Read() && reader.TokenType == JsonToken.PropertyName ) // Consume StartObject, go to PropertyName
      //      {
      //         var influxSeries = new List<InfluxSeries<TInfluxRow>>();

      //         reader.Read(); // Consume PropertyName ("series"), go to StartArray
      //         while ( reader.Read() && reader.TokenType == JsonToken.StartObject ) // Consume StartArray/EndObject, go to StartObject/EndArray
      //         {
      //            string name = null;
      //            var columns = new List<string>();
      //            var dataPoints = new List<TInfluxRow>();
      //            PropertyExpressionInfo<TInfluxRow>[] properties = null;
      //            Dictionary<string, string> tags = null;

      //            while ( reader.Read() && reader.TokenType == JsonToken.PropertyName ) // Consume StartObject/Value, go to PropertyName/EndObject
      //            {
      //               string propertyName = (string)reader.Value;

      //               reader.Read(); // Consume PropertyName, go to Value/StartArray/StartObject

      //               switch ( propertyName )
      //               {
      //                  case "name":
      //                     name = (string)reader.Value;
      //                     break;
      //                  case "columns":
      //                     while ( reader.Read() && reader.TokenType != JsonToken.EndArray ) // Consume StartArray/Value, go to Value/EndArray
      //                     {
      //                        columns.Add( (string)reader.Value );
      //                     }
      //                     properties = new PropertyExpressionInfo<TInfluxRow>[ columns.Count ];
      //                     var propertyMap = TypeCache.GetOrCreateTypeCache<TInfluxRow>().All;
      //                     for ( int i = 0 ; i < columns.Count ; i++ )
      //                     {
      //                        PropertyExpressionInfo<TInfluxRow> propertyInfo;
      //                        if ( !propertyMap.TryGetValue( columns[ i ], out propertyInfo ) )
      //                        {
      //                           throw new InfluxException( $"Could not find the property mapped to the field/tag '{columns[ i ]}' on the type {typeof( TInfluxRow ).Name}." );
      //                        }
      //                        properties[ i ] = propertyInfo;
      //                     }
      //                     break;
      //                  case "tags":
      //                     tags = new Dictionary<string, string>();
      //                     while ( reader.Read() && reader.TokenType != JsonToken.EndObject )
      //                     {
      //                        var tagName = (string)reader.Value;
      //                        reader.Read();
      //                        var tagValue = (string)reader.Value;
      //                        tags.Add( tagName, tagValue );
      //                     }
      //                     break;
      //                  case "values":
      //                     while ( reader.Read() && reader.TokenType != JsonToken.EndArray ) // Consume StartArray/Value, go to Value/EndArray
      //                     {
      //                        var dataPoint = new TInfluxRow();
      //                        var seriesDataPoint = dataPoints as ISeriesDataPoint;
      //                        if ( seriesDataPoint != null )
      //                        {
      //                           seriesDataPoint.SeriesName = name;
      //                        }

      //                        int i = 0;
      //                        while ( reader.Read() && reader.TokenType != JsonToken.EndArray ) // Consume StartArray/Value, go to Value/EndArray
      //                        {
      //                           properties[ i ]?.SetValue( dataPoint, reader.Value ); // WHAT VALUE? Will probably work....?

      //                           i++;
      //                        }

      //                        dataPoints.Add( dataPoint );
      //                     }
      //                     break;
      //                  default:
      //                     throw new InfluxException( "An unknown exception occurred during query parsing." );
      //               }

      //            }

      //            // can construct object
      //            influxSeries.Add( new InfluxSeries<TInfluxRow>( name, dataPoints, tags ) ); // ADD TAGS

      //         }

      //         results.Add( new InfluxQueryResult<TInfluxRow>( influxSeries ) );
      //      }

      //   }

      //   return new InfluxQueryResultSet<TInfluxRow>( results );
      //}
   }
}
