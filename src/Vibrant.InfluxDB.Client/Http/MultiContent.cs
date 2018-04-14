using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Http
{
   internal sealed class MultiContent : HttpContent
   {
      private IEnumerable<HttpContent> _contents;

      public MultiContent( IEnumerable<HttpContent> contents )
      {
         _contents = contents;
      }

      protected async override Task SerializeToStreamAsync( Stream stream, TransportContext context )
      {
         foreach( var content in _contents )
         {
            await content.CopyToAsync( stream ).ConfigureAwait( false );
         }
      }

      protected override bool TryComputeLength( out long length )
      {
         length = -1;
         return false;
      }
   }
}
