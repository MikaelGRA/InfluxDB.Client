using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Parsers;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal class QueryResultIterator<TInfluxRow> : IDisposable
      where TInfluxRow : new()
   {
      private readonly HttpResponseMessage _responseMessage; 
      private readonly JsonStreamObjectIterator _objectIterator;
      private readonly InfluxQueryOptions _options;
      private readonly InfluxClient _client;
      private readonly string _db;

      private InfluxResultSet<TInfluxRow> _currentResultSet;
      private int _currentResultIndex;
      private int _currentSerieIndex;

      private bool _hasConsumedAllQueryResults;
      private bool _disposed = false;

      internal QueryResultIterator( HttpResponseMessage responseMessage, JsonStreamObjectIterator objectIterator, InfluxClient client, InfluxQueryOptions options, string db )
      {
         _responseMessage = responseMessage;
         _objectIterator = objectIterator;
         _client = client;
         _options = options;
         _db = db;
      }

      private async Task<bool> ConsumeNextQueryResultAsync()
      {
         // read the next object from the object iterator (stream)
         var queryResult = _objectIterator.ReadNext<QueryResult>();
         if( queryResult == null )
         {
            _currentResultSet = null;
            return false;
         }

         // if we found something, construct a normal result object, and initialize our indices
         _currentResultSet = await ResultSetFactory.CreateAsync<TInfluxRow>( _client, new[] { queryResult }, _db, true, _options ).ConfigureAwait( false );
         _currentResultIndex = -1;
         _currentSerieIndex = -1;

         // indicate we found something
         return true;
      }

      private async Task<bool> ConsumeNextResultSetAsync()
      {
         // short circuit, so we dont go to the stream after we already found there is no more data
         if( _hasConsumedAllQueryResults ) return false;
         
         // consume the next query result and store it in this instance
         var hasMore = await ConsumeNextQueryResultAsync().ConfigureAwait( false );
         _hasConsumedAllQueryResults = !hasMore;

         // indicate we found something
         return hasMore;
      }

      public async Task<bool> ConsumeNextResultAsync()
      {
         bool hasMore;
         if( _currentResultSet == null )
         {
            hasMore = await ConsumeNextResultSetAsync().ConfigureAwait( false );
            if( !hasMore )
            {
               return false;
            }
         }

         // advance to next result/series
         _currentResultIndex++;
         _currentSerieIndex = -1;

         // within bounds?
         if( _currentResultIndex < CurrentResultSet.Results.Count )
         {
            return true;
         }

         // if we get to here, it is an indication that there is no more iterable data available in the current result set
         hasMore = await ConsumeNextResultSetAsync().ConfigureAwait( false );
         if( !hasMore )
         {
            return false;
         }

         // advance to next result/series
         _currentResultIndex++;
         _currentSerieIndex = -1;

         // within bounds?
         if( _currentResultIndex < CurrentResultSet.Results.Count )
         {
            return true;
         }

         return false;
      }

      public async Task<bool> ConsumeNextSerieAsync()
      {
         bool hasMore;
         if( _currentResultSet == null )
         {
            hasMore = await ConsumeNextResultAsync().ConfigureAwait( false );
            if( !hasMore )
            {
               return false;
            }
         }

         // advance to next series
         _currentSerieIndex++;

         // within bounds?
         if( _currentSerieIndex < CurrentResult.Series.Count )
         {
            return true;
         }

         return false;
      }

      private InfluxResultSet<TInfluxRow> CurrentResultSet
      {
         get
         {
            return _currentResultSet;
         }
      }

      public InfluxResult<TInfluxRow> CurrentResult
      {
         get
         {
            return _currentResultSet.Results[ _currentResultIndex ];
         }
      }

      public InfluxSeries<TInfluxRow> CurrentSerie
      {
         get
         {
            return CurrentResult.Series[ _currentSerieIndex ];
         }
      }
      
      protected virtual void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            if( disposing )
            {
               _responseMessage.Dispose();
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
