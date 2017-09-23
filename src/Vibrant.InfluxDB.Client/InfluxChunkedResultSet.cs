using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Parsers;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunkedResultSet<TInfluxRow> : IDisposable
      where TInfluxRow : new()
   {
      private ContextualQueryResultIterator<TInfluxRow> _iterator;

      private bool _disposed = false;

      internal InfluxChunkedResultSet( ContextualQueryResultIterator<TInfluxRow> contextualIterator, InfluxClient client, InfluxQueryOptions options, string db )
      {
         _iterator = contextualIterator;
      }

      /// <summary>
      /// Gets the next result from the result set.
      /// 
      /// Null if none are available.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxChunkedResult<TInfluxRow>> GetNextResultAsync()
      {
         if( _iterator == null ) return null;

         if( await _iterator.ConsumeNextResultAsync().ConfigureAwait( false ) )
         {
            var currentResult = _iterator.CurrentResult;

            return new InfluxChunkedResult<TInfluxRow>(
               _iterator,
               currentResult.StatementId,
               currentResult.ErrorMessage );
         }

         // here we can close it
         _iterator.Dispose();
         _iterator = null;

         return null;
      }

      private void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            if( disposing )
            {
               if(_iterator != null )
               {
                  _iterator.Dispose();
                  _iterator = null;
               }
            }

            _disposed = true;
         }
      }
      
      public void Dispose()
      {
         Dispose( true );
      }
   }
}
