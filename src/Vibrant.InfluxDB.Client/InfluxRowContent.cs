using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Helpers;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxRowContent<TInfluxRow> : HttpContent
      where TInfluxRow : new()
   {
      private static readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };
      private static readonly Encoding UTF8 = new UTF8Encoding( false );

      private readonly IEnumerable<TInfluxRow> _dataPoints;
      private readonly Func<TInfluxRow, string> _getMeasurementName;
      private readonly TimestampPrecision _precision;

      public InfluxRowContent( IEnumerable<TInfluxRow> dataPoints, Func<TInfluxRow, string> getMeasurementName, TimestampPrecision precision )
      {
         _dataPoints = dataPoints;
         _getMeasurementName = getMeasurementName;
         _precision = precision;

         Headers.ContentType = _mediaType;
      }

      protected override Task SerializeToStreamAsync( Stream stream, TransportContext context )
      {
         var precision = _precision;
         var getMeasurementName = _getMeasurementName;

         if ( QueryTransform.IsCustomDataPoint<TInfluxRow>() )
         {

            var writer = new StreamWriter( stream, UTF8 );
            foreach ( IInfluxRow dp in _dataPoints )
            {
               writer.Write( getMeasurementName( (TInfluxRow)dp ) );
               foreach ( var kvp in dp.GetAllTags() )
               {
                  var value = kvp.Value;
                  if ( value != null )
                  {
                     writer.Write( "," );
                     writer.Write( kvp.Key );
                     writer.Write( "=" );
                     writer.Write( value );
                  }
               }
               writer.Write( " " );

               using ( var enumerator = dp.GetAllFields().GetEnumerator() ) // exclude time????????
               {
                  bool hasMore = enumerator.MoveNext();
                  bool hasValue = false;
                  object value = null;
                  KeyValuePair<string, object> current = default( KeyValuePair<string, object> );
                  while ( hasMore && !hasValue )
                  {
                     current = enumerator.Current;
                     value = enumerator.Current.Value;
                     hasValue = value != null;

                     hasMore = enumerator.MoveNext();
                  }

                  while ( hasValue )
                  {
                     writer.Write( current.Key );
                     writer.Write( "=" );
                     writer.Write( Convert.ToString( value, CultureInfo.InvariantCulture ) );

                     // get a hold of the next non-null value
                     hasValue = false;
                     while ( hasMore && !hasValue )
                     {
                        current = enumerator.Current;
                        value = enumerator.Current.Value;
                        hasValue = value != null;

                        hasMore = enumerator.MoveNext();
                     }

                     // we have just written a value, and now we have the next non-null value
                     if ( hasValue )
                     {
                        writer.Write( "," );
                     }
                  }
               }
               writer.Write( " " );

               var ts = dp.ReadTimestamp();
               long ticks = ts.ToPrecision( precision );
               writer.Write( ticks );
               writer.Write( "\n" );
            }
            writer.Flush();
         }
         else
         {
            var cache = TypeCache.GetOrCreateTypeCache<TInfluxRow>();
            var tags = cache.Tags;
            var fields = cache.Fields;
            var timestamp = cache.Timestamp;
            if ( timestamp == null )
            {
               throw new InvalidOperationException( "Cannot serialize data points without an influx timestamp" );
            }

            var writer = new StreamWriter( stream, UTF8 );
            foreach ( var dp in _dataPoints )
            {
               writer.Write( getMeasurementName( dp ) );
               if ( tags.Count > 0 )
               {
                  foreach ( var kvp in tags )
                  {
                     var value = kvp.Value.GetValue( dp );
                     if ( value != null )
                     {
                        writer.Write( "," );
                        writer.Write( kvp.Key );
                        writer.Write( "=" );
                        writer.Write( value );
                     }
                  }
               }
               writer.Write( " " );

               using ( var enumerator = fields.GetEnumerator() )
               {
                  bool hasMore = enumerator.MoveNext();
                  bool hasValue = false;
                  object value = null;
                  KeyValuePair<string, PropertyExpressionInfo<TInfluxRow>> current = default( KeyValuePair<string, PropertyExpressionInfo<TInfluxRow>> );
                  while ( hasMore && !hasValue )
                  {
                     current = enumerator.Current;
                     value = current.Value.GetValue( dp );
                     hasValue = value != null;

                     hasMore = enumerator.MoveNext();
                  }

                  while ( hasValue )
                  {
                     writer.Write( current.Key );
                     writer.Write( "=" );
                     writer.Write( Convert.ToString( value, CultureInfo.InvariantCulture ) );

                     // get a hold of the next non-null value
                     hasValue = false;
                     while ( hasMore && !hasValue )
                     {
                        current = enumerator.Current;
                        value = current.Value.GetValue( dp );
                        hasValue = value != null;

                        hasMore = enumerator.MoveNext();
                     }

                     // we have just written a value, and now we have the next non-null value
                     if ( hasValue )
                     {
                        writer.Write( "," );
                     }
                  }
               }
               writer.Write( " " );

               var ts = (DateTime)timestamp.GetValue( dp );
               long ticks = ts.ToPrecision( precision );
               if ( ticks < 0 )
               {
                  throw new InfluxException( "Timestamp cannot be earlier than epoch (1. Jan. 1970)." );
               }
               writer.Write( ticks );
               writer.Write( "\n" );
            }
            writer.Flush();
         }
         return Task.FromResult( 0 );
      }

      protected override bool TryComputeLength( out long length )
      {
         length = -1;
         return false;
      }
   }
}
