using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Http
{
   internal class InfluxRowContent<TInfluxRow, TTimestamp> : HttpContent
      where TInfluxRow : new()
   {
      private static readonly MediaTypeHeaderValue TextMediaType = new MediaTypeHeaderValue( "text/plain" ) { CharSet = "utf-8" };
      private static readonly Encoding UTF8 = new UTF8Encoding( false );
      private readonly bool _isBasedOnInterface;
      private readonly IEnumerable _dataPoints;
      private readonly Func<TInfluxRow, string> _getMeasurementName;
      private readonly InfluxWriteOptions _options;
      private readonly ITimestampParser<TTimestamp> _timestampParser;

      internal InfluxRowContent( InfluxClient client, bool isBasedOnInterface, IEnumerable dataPoints, Func<TInfluxRow, string> getMeasurementName, InfluxWriteOptions options )
      {
         _isBasedOnInterface = isBasedOnInterface;
         _dataPoints = dataPoints;
         _getMeasurementName = getMeasurementName;
         _options = options;
         _timestampParser = client.TimestampParserRegistry.FindTimestampParserOrNull<TTimestamp>();

         Headers.ContentType = TextMediaType;
      }

      protected override Task SerializeToStreamAsync( Stream stream, TransportContext context )
      {
         var writer = new StreamWriter( stream, UTF8 );

         var precision = _options.Precision;
         var getMeasurementName = _getMeasurementName;

         if ( _isBasedOnInterface )
         {
            foreach ( IInfluxRow<TTimestamp> dp in _dataPoints )
            {
               // write measurement name
               writer.Write( LineProtocolEscape.EscapeMeasurementName( getMeasurementName( (TInfluxRow)dp ) ) ); // FIXME: Escape?

               // write all tags
               foreach ( var kvp in dp.GetAllTags() ) // Ensure tags are in correct order?
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
                  long ticks = _timestampParser.ToEpoch( precision, ts );
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

            foreach ( TInfluxRow dp in _dataPoints )
            {
               // write measurement name
               writer.Write( LineProtocolEscape.EscapeMeasurementName( getMeasurementName( dp ) ) );

               // write all tags
               if ( tags.Count > 0 )
               {
                  foreach ( var tagProperty in tags )
                  {
                     var value = tagProperty.GetValue( dp );
                     if ( value != null )
                     {
                        writer.Write( ',' );
                        writer.Write( tagProperty.LineProtocolEscapedKey );
                        writer.Write( '=' );
                        writer.Write( LineProtocolEscape.EscapeTagValue( tagProperty.GetTagString( value ) ) );
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
                  while ( hasMore && !hasValue )
                  {
                     property = enumerator.Current;
                     value = property.GetValue( dp );
                     hasValue = value != null;

                     hasMore = enumerator.MoveNext();
                  }

                  while ( hasValue )
                  {
                     writer.Write( property.LineProtocolEscapedKey );
                     writer.Write( '=' );
                     if ( property.IsEnum )
                     {
                        writer.Write( LineProtocolEscape.EscapeFieldValue( property.GetFieldString( value ) ) );
                     }
                     else
                     {
                        writer.Write( LineProtocolEscape.EscapeFieldValue( value ) );
                     }

                     // get a hold of the next non-null value
                     hasValue = false;
                     while ( hasMore && !hasValue )
                     {
                        property = enumerator.Current;
                        value = property.GetValue( dp );
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
                     long ticks = _timestampParser.ToEpoch( precision, (TTimestamp)ts );
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
