using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Metadata;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class LineProtocolEscape
   {
      private static readonly IDictionary<string, string> EscapedKeys = new ConcurrentDictionary<string, string>();

      internal static string EscapeFieldValue( object valueAsObject )
      {
         if( valueAsObject is double )
         {
            return ( (double)valueAsObject ).ToString( CultureInfo.InvariantCulture );
         }
         else if ( valueAsObject is long || valueAsObject is int || valueAsObject is short || valueAsObject is byte || valueAsObject is ulong || valueAsObject is uint || valueAsObject is ushort || valueAsObject is sbyte )
         {
            return valueAsObject.ToString() + 'i';
         }
         else if ( valueAsObject is string )
         {
            return '\"' + ( (string)valueAsObject ).Replace( "\"", "\\\"" ) + '\"';
         }
         else if ( valueAsObject is bool )
         {
            return ( (bool)valueAsObject ) ? InfluxConstants.True : InfluxConstants.False;
         }
         else if( valueAsObject is float )
         {
            return ( (float)valueAsObject ).ToString( CultureInfo.InvariantCulture );
         }
         else if ( valueAsObject is DateTime )
         {
            var valueAsDateTime = (DateTime)valueAsObject;

            return '\"' + valueAsDateTime.ToIso8601() + '\"';
         }
         else
         {
            return Convert.ToString( valueAsObject, CultureInfo.InvariantCulture );
         }
      }

      internal static string EscapeTagValue( string value )
      {
         return Escape( value );
      }

      internal static string EscapeKey( string value )
      {
         // attempt to get a cached value here, as there will be a limited set of different tags
         string cachedValue;
         if ( !EscapedKeys.TryGetValue( value, out cachedValue ) )
         {
            cachedValue = Escape( value );
            EscapedKeys[ value ] = cachedValue;
         }
         return cachedValue;
      }

      private static string Escape( string value )
      {
         // https://docs.influxdata.com/influxdb/v0.9/write_protocols/line/

         StringBuilder builder = new StringBuilder( value.Length );
         for ( int i = 0 ; i < value.Length ; i++ )
         {
            var c = value[ i ];
            switch ( c )
            {
               case ',':
                  builder.Append( "\\," );
                  break;
               case ' ':
                  builder.Append( "\\ " );
                  break;
               case '=':
                  builder.Append( "\\=" );
                  break;
               default:
                  builder.Append( c );
                  break;
            }
         }
         return builder.ToString();
      }

      public static string EscapeMeasurementName( string value )
      {
         // https://docs.influxdata.com/influxdb/v1.2/write_protocols/line_protocol_tutorial/#special-characters-and-keywords

         StringBuilder builder = new StringBuilder( value.Length );
         for( int i = 0 ; i < value.Length ; i++ )
         {
            var c = value[ i ];
            switch( c )
            {
               case ',':
                  builder.Append( "\\," );
                  break;
               case ' ':
                  builder.Append( "\\ " );
                  break;
               default:
                  builder.Append( c );
                  break;
            }
         }
         return builder.ToString();
      }
   }
}
