using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal static class UriHelper
   {
      public static string SafeEscapeDataString( string data )
      {
         if( data.Length < 32766 )
         {
            return Uri.EscapeDataString( data );
         }
         var escaped = new StringBuilder();
         for( var i = 0 ; i < data.Length ; i += 32765 )
         {
            var length = data.Length - i <= 32765 ? data.Length - i : 32765;
            escaped.Append( Uri.EscapeDataString( data.Substring( i, length ) ) );
         }
         return escaped.ToString().Replace( "%20", "+" );
      }
   }
}
