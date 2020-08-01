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
using Vibrant.InfluxDB.Client.Http;
using System.Collections.Concurrent;
using System.Threading;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class ResultSetFactory
   {
      private static readonly DateTimeStyles OnlyUtcStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
      private static readonly MethodInfo CreateBasedOnAttributesMethod = typeof( ResultSetFactory ).GetMethod( "CreateBasedOnAttributes", BindingFlags.Static | BindingFlags.NonPublic );
      private static readonly MethodInfo CreateBasedOnInterfaceAsyncMethod = typeof( ResultSetFactory ).GetMethod( "CreateBasedOnInterfaceAsync", BindingFlags.Static | BindingFlags.NonPublic );


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
         bool allowMetadataQuerying,
         InfluxQueryOptions options,
         CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         var propertyMap = MetadataCache.GetOrCreate<TInfluxRow>();
         var timestampType = propertyMap.GetTimestampType();
         if( propertyMap.IsBasedOnInterface() )
         {
            var createBasedOnInterfaceAsync = CreateBasedOnInterfaceAsyncMethod.MakeGenericMethod( new[] { typeof( TInfluxRow ), timestampType } );
            return (Task<InfluxResultSet<TInfluxRow>>)createBasedOnInterfaceAsync.Invoke( 
                null, 
                new object[] { client, queryResult, db, allowMetadataQuerying, propertyMap, options, cancellationToken } );
         }
         else
         {
            var createBasedOnAttributes = CreateBasedOnAttributesMethod.MakeGenericMethod( new[] { typeof( TInfluxRow ), timestampType } );
            return Task.FromResult( (InfluxResultSet<TInfluxRow>)createBasedOnAttributes.Invoke( null, new object[] { client, queryResult, options, propertyMap } ) );
         }
      }

      private static InfluxResultSet<TInfluxRow> CreateBasedOnAttributes<TInfluxRow, TTimestamp>(
         InfluxClient client,
         IEnumerable<QueryResult> queryResults,
         InfluxQueryOptions options,
         InfluxRowTypeInfo<TInfluxRow> propertyMap )
         where TInfluxRow : new()
      {
         // Create type based on attributes
         Dictionary<int, InfluxResult<TInfluxRow>> results = new Dictionary<int, InfluxResult<TInfluxRow>>();
         var timestampParser = client.TimestampParserRegistry.FindTimestampParserOrNull<TTimestamp>();

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

                     // create new dictionary, with correct typing (we must potentially convert strings to enums, if the column being used in the GROUP BY is an enum)
                     var tags = CreateTagDictionaryFromSerieBasedOnAttributes( series, propertyMap.All );

                     // find or create influx serie
                     var influxSerie = existingResult.FindGroupInternal( name, tags, true );
                     if( influxSerie == null )
                     {
                        influxSerie = new InfluxSeries<TInfluxRow>( name, tags );
                        existingResult.Series.Add( influxSerie );
                     }

                     // add data to found serie
                     AddValuesToInfluxSeriesByAttributes<TInfluxRow, TTimestamp>( influxSerie, series, propertyMap, options, timestampParser );
                  }
               }
            }
         }

         return new InfluxResultSet<TInfluxRow>( results.Values.ToList() );
      }

      private static void AddValuesToInfluxSeriesByAttributes<TInfluxRow, TTimestamp>(
         InfluxSeries<TInfluxRow> influxSerie,
         SeriesResult series,
         InfluxRowTypeInfo<TInfluxRow> propertyMap,
         InfluxQueryOptions options,
         ITimestampParser<TTimestamp> timestampParser )
         where TInfluxRow : new()
      {
         // Values will be null, if there are no entries in the result set
         if( series.Values != null )
         {
            var dataPoints = new List<TInfluxRow>();

            var precision = options.Precision;
            var columns = series.Columns;
            var name = series.Name;

            // construct an array of properties based on the same indexing as the columns returned by the query
            var properties = new PropertyExpressionInfo<TInfluxRow>[ columns.Count ];
            for( int i = 0 ; i < columns.Count ; i++ )
            {
               PropertyExpressionInfo<TInfluxRow> propertyInfo;
               if( propertyMap.All.TryGetValue( columns[ i ], out propertyInfo ) )
               {
                  properties[ i ] = propertyInfo;
               }
            }

            foreach( var values in series.Values )
            {
               // construct the data points based on the attributes
               var dataPoint = new TInfluxRow();
               propertyMap.SetMeasurementName( name, dataPoint );

               for( int i = 0 ; i < values.Count ; i++ )
               {
                  var value = values[ i ];
                  var property = properties[ i ];

                  // set the value based on the property, if both the value and property is not null
                  if( property != null )
                  {
                     if( value != null )
                     {
                        if( property.Key == InfluxConstants.TimeColumn )
                        {
                           property.SetValue( dataPoint, timestampParser.ToTimestamp( options.Precision, value ) );
                        }
                        else if( property.IsDateTime )
                        {
                           property.SetValue( dataPoint, DateTime.Parse( (string)value, CultureInfo.InvariantCulture, OnlyUtcStyles ) );
                        }
                        else if( property.IsDateTimeOffset )
                        {
                           property.SetValue( dataPoint, DateTimeOffset.Parse( (string)value, CultureInfo.InvariantCulture ) );
                        }
                        else if( property.IsEnum )
                        {
                           property.SetValue( dataPoint, property.GetEnumValue( value ) );
                        }
                        else
                        {
                           if( value.GetType() == property.Type )
                           {
                              property.SetValue( dataPoint, value );
                           }
                           else if( value is string stringValue )
                           {
                              if( !string.IsNullOrEmpty( stringValue ) )
                              {
                                 property.SetValue( dataPoint, Convert.ChangeType( stringValue, property.Type, CultureInfo.InvariantCulture ) );
                              }
                           }
                           else
                           {
                              property.SetValue( dataPoint, Convert.ChangeType( value, property.Type, CultureInfo.InvariantCulture ) );
                           }
                        }
                     }
                  }
               }

               dataPoints.Add( dataPoint );
            }

            influxSerie.Rows.AddRange( dataPoints );
         }
      }

      private static Dictionary<string, object> CreateTagDictionaryFromSerieBasedOnAttributes<TInfluxRow>(
         SeriesResult series,
         IReadOnlyDictionary<string, PropertyExpressionInfo<TInfluxRow>> propertyMap )
      {
         Dictionary<string, object> tags = new Dictionary<string, object>();
         if( series.Tags != null )
         {
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
                        value = Convert.ChangeType( kvp.Value, property.Type, CultureInfo.InvariantCulture );
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
         return tags;
      }

      private static async Task<InfluxResultSet<TInfluxRow>> CreateBasedOnInterfaceAsync<TInfluxRow, TTimestamp>(
         InfluxClient client,
         IEnumerable<QueryResult> queryResults,
         string db,
         bool allowMetadataQuerying,
         InfluxRowTypeInfo<TInfluxRow> propertyMap,
         InfluxQueryOptions options,
         CancellationToken cancellationToken = default)
         where TInfluxRow : IInfluxRow<TTimestamp>, new()
      {
         var timestampParser = client.TimestampParserRegistry.FindTimestampParserOrNull<TTimestamp>();

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

                     // create influx series
                     var tags = series.Tags?.ToDictionary( x => x.Key, x => x.Value == string.Empty ? null : (object)x.Value ) ?? new Dictionary<string, object>();

                     // find or create influx serie
                     var influxSerie = existingResult.FindGroupInternal( name, tags, true );
                     if( influxSerie == null )
                     {
                        influxSerie = new InfluxSeries<TInfluxRow>( name, tags );
                        existingResult.Series.Add( influxSerie );
                     }

                     // add data to found series
                     await AddValuesToInfluxSeriesByInterfaceAsync<TInfluxRow, TTimestamp>(
                         influxSerie, series, client, db, allowMetadataQuerying, propertyMap, options, timestampParser, cancellationToken );
                  }
               }
            }
         }

         return new InfluxResultSet<TInfluxRow>( results.Values.ToList() );
      }


      private static async Task AddValuesToInfluxSeriesByInterfaceAsync<TInfluxRow, TTimestamp>(
         InfluxSeries<TInfluxRow> influxSerie,
         SeriesResult series,
         InfluxClient client,
         string db,
         bool allowMetadataQuerying,
         InfluxRowTypeInfo<TInfluxRow> propertyMap,
         InfluxQueryOptions options,
         ITimestampParser<TTimestamp> timestampParser,
         CancellationToken cancellationToken = default)
         where TInfluxRow : IInfluxRow<TTimestamp>, new()
      {
         // Values will be null, if there are no entries in the result set
         if( series.Values != null )
         {
            var precision = options.Precision;
            var name = series.Name;
            var columns = series.Columns;
            var setters = new Action<TInfluxRow, string, object>[ columns.Count ];
            var dataPoints = new List<TInfluxRow>();
            // Get metadata information about the measurement we are querying, as we dont know
            // which columns are tags/fields otherwise

            DatabaseMeasurementInfo meta = null;
            if( allowMetadataQuerying )
            {
               meta = await client.GetMetaInformationAsync( db, name, options.MetadataExpiration, cancellationToken ).ConfigureAwait( false );
            }

            for( int i = 0 ; i < columns.Count ; i++ )
            {
               var columnName = columns[ i ];

               if( !allowMetadataQuerying )
               {
                  setters[ i ] = ( row, fieldName, value ) => row.SetField( fieldName, value );
               }
               else if( columnName == InfluxConstants.TimeColumn )
               {
                  setters[ i ] = ( row, timeName, value ) => row.SetTimestamp( timestampParser.ToTimestamp( options.Precision, value ) );
               }
               else if( meta.Tags.Contains( columnName ) )
               {
                  setters[ i ] = ( row, tagName, value ) => row.SetTag( tagName, (string)value );
               }
               else
               {
                  setters[ i ] = ( row, fieldName, value ) => row.SetField( fieldName, value );
               }
            }

            // constructs the IInfluxRows using the IInfluxRow interface
            foreach( var values in series.Values )
            {
               var dataPoint = new TInfluxRow();
               propertyMap.SetMeasurementName( name, dataPoint );

               // go through all values that are stored as a List<List<object>>
               for( int i = 0 ; i < values.Count ; i++ )
               {
                  var value = values[ i ]; // TODO: What about NULL values? Are they treated as empty strings or actual nulls?
                  if( value != null )
                  {
                     setters[ i ]( dataPoint, columns[ i ], value );
                  }
               }

               dataPoints.Add( dataPoint );
            }

            influxSerie.Rows.AddRange( dataPoints );
         }
      }
   }
}
