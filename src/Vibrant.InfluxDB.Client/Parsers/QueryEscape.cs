using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Parsers
{
   internal static class QueryEscape
   {
      internal static string EscapeFieldOrTag( object valueAsObject )
      {
         if( valueAsObject is long || valueAsObject is ulong || valueAsObject is int || valueAsObject is uint || valueAsObject is short || valueAsObject is ushort || valueAsObject is byte || valueAsObject is sbyte )
         {
            return valueAsObject.ToString() + 'i';
         }
         else if( valueAsObject is string )
         {
            string value = (string)valueAsObject;
            var builder = new StringBuilder( value.Length + 2 );
            builder.Append( '\'' );
            for( int i = 0 ; i < value.Length ; i++ )
            {
               var c = value[ i ];
               switch( c )
               {
                  case '\'':
                     builder.Append( "\\'" );
                     break;
                  default:
                     builder.Append( c );
                     break;
               }
            }
            builder.Append( '\'' );
            return builder.ToString();
         }
         else if( valueAsObject is bool )
         {
            return ( (bool)valueAsObject ) ? InfluxConstants.True : InfluxConstants.False;
         }
         else if( valueAsObject is DateTime )
         {
            var valueAsDateTime = (DateTime)valueAsObject;

            return '\'' + valueAsDateTime.ToIso8601() + '\'';
         }
         else
         {
            return Convert.ToString( valueAsObject, CultureInfo.InvariantCulture );
         }
      }
   }
}
