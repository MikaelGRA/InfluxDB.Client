using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client.Http
{
   internal class LongFormUrlEncodedContent : ByteArrayContent
   {
      private static readonly Encoding WesternEuropeanEncoding = Encoding.GetEncoding( 28591 );

      public LongFormUrlEncodedContent( IEnumerable<KeyValuePair<string, string>> nameValueCollection )
          : base( GetContentByteArray( nameValueCollection ) )
      {
         Headers.ContentType = new MediaTypeHeaderValue( "application/x-www-form-urlencoded" );
      }

      private static byte[] GetContentByteArray( IEnumerable<KeyValuePair<string, string>> nameValueCollection )
      {
         if( nameValueCollection == null )
         {
            throw new ArgumentNullException( nameof( nameValueCollection ) );
         }

         // Encode and concatenate data
         StringBuilder builder = new StringBuilder();
         foreach( KeyValuePair<string, string> pair in nameValueCollection )
         {
            if( builder.Length > 0 )
            {
               builder.Append( '&' );
            }

            builder.Append( Encode( pair.Key ) );
            builder.Append( '=' );
            builder.Append( Encode( pair.Value ) );
         }

         return WesternEuropeanEncoding.GetBytes( builder.ToString() );
      }

      private static string Encode( string data )
      {
         if( string.IsNullOrEmpty( data ) )
         {
            return string.Empty;
         }
         // Escape spaces as '+'.
         return UriHelper.SafeEscapeDataString( data );
      }
   }
}
