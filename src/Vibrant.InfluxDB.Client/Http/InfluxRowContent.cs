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
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Http
{
   internal class InfluxRowContent<TInfluxRow> : HttpContent
      where TInfluxRow : new()
   {
      private static readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };
      private static readonly Encoding UTF8 = new UTF8Encoding( false );

      private readonly IEnumerable<TInfluxRow> _dataPoints;
      private readonly Func<TInfluxRow, string> _getMeasurementName;
      private readonly TimestampPrecision _precision;

      internal InfluxRowContent( IEnumerable<TInfluxRow> dataPoints, Func<TInfluxRow, string> getMeasurementName, TimestampPrecision precision )
      {
         _dataPoints = dataPoints;
         _getMeasurementName = getMeasurementName;
         _precision = precision;

         Headers.ContentType = _mediaType;
      }

      protected override Task SerializeToStreamAsync( Stream stream, TransportContext context )
      {
         var writer = new StreamWriter( stream, UTF8 );

         var precision = _precision;
         var getMeasurementName = _getMeasurementName;

         if ( ResultSetFactory.IsIInfluxRow<TInfluxRow>() )
         {
            foreach ( IInfluxRow dp in _dataPoints )
            {
               // write measurement name
               writer.Write( getMeasurementName( (TInfluxRow)dp ) );

               // write all tags
               foreach ( var kvp in dp.GetAllTags() )
               {
                  var value = kvp.Value;
                  if ( value != null )
                  {
                     writer.Write( ',' );
                     writer.Write( LineProtocolEscape.EscapeKey( kvp.Key ) );
                     writer.Write( '=' );
                     writer.Write( LineProtocolEscape.EscapeTagValue( value ) );
                  }
               }

               // write tag to field seperator
               writer.Write( ' ' );

               // write all fields
               using ( var enumerator = dp.GetAllFields().GetEnumerator() )
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
                     writer.Write( LineProtocolEscape.EscapeKey( current.Key ) );
                     writer.Write( '=' );
                     writer.Write( LineProtocolEscape.EscapeFieldValue( value ) );

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
                        writer.Write( ',' );
                     }
                  }
               }
               
               // write timestamp, if exists
               var ts = dp.GetTimestamp();
               if ( ts != null )
               {
                  writer.Write( ' ' );
                  long ticks = ts.Value.ToPrecision( precision );
                  writer.Write( ticks );
               }

               writer.Write( '\n' );
            }
         }
         else
         {
            var cache = MetadataCache.GetOrCreate<TInfluxRow>();
            var tags = cache.Tags;
            var fields = cache.Fields;
            var timestamp = cache.Timestamp;

            foreach ( var dp in _dataPoints )
            {
               // write measurement name
               writer.Write( getMeasurementName( dp ) );

               // write all tags
               if ( tags.Count > 0 )
               {
                  foreach ( var kvp in tags )
                  {
                     var property = kvp.Value;
                     var value = property.GetValue( dp );
                     if ( value != null )
                     {
                        writer.Write( ',' );
                        writer.Write( property.EscapedKey );
                        writer.Write( '=' );
                        writer.Write( LineProtocolEscape.EscapeTagValue( property.GetStringValue( value ) ) );
                     }
                  }
               }

               // write tag to fields seperator
               writer.Write( ' ' );

               // write all fields
               using ( var enumerator = fields.GetEnumerator() )
               {
                  bool hasMore = enumerator.MoveNext();
                  bool hasValue = false;
                  object value = null;
                  PropertyExpressionInfo<TInfluxRow> property = null;
                  KeyValuePair<string, PropertyExpressionInfo<TInfluxRow>> current = default( KeyValuePair<string, PropertyExpressionInfo<TInfluxRow>> );
                  while ( hasMore && !hasValue )
                  {
                     current = enumerator.Current;
                     property = current.Value;
                     value = current.Value.GetValue( dp );
                     hasValue = value != null;

                     hasMore = enumerator.MoveNext();
                  }

                  while ( hasValue )
                  {
                     writer.Write( property.EscapedKey );
                     writer.Write( '=' );
                     if ( property.IsEnum )
                     {
                        writer.Write( LineProtocolEscape.EscapeFieldValue( property.GetStringValue( value ) ) );
                     }
                     else
                     {
                        writer.Write( LineProtocolEscape.EscapeFieldValue( value ) );
                     }

                     // get a hold of the next non-null value
                     hasValue = false;
                     while ( hasMore && !hasValue )
                     {
                        current = enumerator.Current;
                        property = current.Value;
                        value = current.Value.GetValue( dp );
                        hasValue = value != null;

                        hasMore = enumerator.MoveNext();
                     }

                     // we have just written a value, and now we have the next non-null value
                     if ( hasValue )
                     {
                        writer.Write( ',' );
                     }
                  }
               }

               // write timestamp, if exists
               if ( timestamp != null )
               {
                  var ts = timestamp.GetValue( dp );
                  if ( ts != null )
                  {
                     writer.Write( ' ' );
                     long ticks = ( (DateTime)ts ).ToPrecision( precision );
                     writer.Write( ticks );
                  }
               }

               writer.Write( '\n' );
            }
         }

         writer.Flush();

         return Task.FromResult( 0 );
      }

      protected override bool TryComputeLength( out long length )
      {
         length = -1;
         return false;
      }
   }
}
