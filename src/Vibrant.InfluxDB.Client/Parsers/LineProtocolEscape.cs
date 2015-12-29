using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class LineProtocolEscape
   {
      private static readonly IDictionary<string, string> EscapedKeys = new ConcurrentDictionary<string, string>();

      private const string True = "true";
      private const string False = "false";

      internal static string EscapeFieldValue( object valueAsObject )
      {
         if ( valueAsObject is long || valueAsObject is int )
         {
            return valueAsObject + "i";
         }
         else if ( valueAsObject is string )
         {
            return "\"" + ( (string)valueAsObject ).Replace( "\"", "\\\"" ) + "\"";
         }
         else if ( valueAsObject is bool )
         {
            return ( (bool)valueAsObject ) ? True : False;
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
         string cachedValue;
         if ( !EscapedKeys.TryGetValue( value, out cachedValue ) )
         {
            cachedValue = Escape( value );
            EscapedKeys.Add( value, cachedValue );
         }
         return cachedValue;
      }

      private static string Escape( string value )
      {
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
   }
}
