using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Parsers;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunkedResultSet<TInfluxRow>
      where TInfluxRow : new()
   {
      private QueryResultIterator<TInfluxRow> _iterator;
      private InfluxResultSet<TInfluxRow> _currentSet;

      private int? _currentStatementId;

      internal InfluxChunkedResultSet( JsonStreamObjectIterator objectIterator, InfluxClient client, InfluxQueryOptions options, string db )
      {
         _iterator = new QueryResultIterator<TInfluxRow>( objectIterator, client, options, db );
      }

      public async Task<InfluxChunkedResult<TInfluxRow>> GetNextResultAsync()
      {
         bool hasMore = false;
         if( !_currentStatementId.HasValue )
         {
            hasMore = await _iterator.ConsumeNextAsync();
         }


      }
   }
}
