using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   internal class ContextualQueryResultIterator<TInfluxRow> : IDisposable
      where TInfluxRow : new()
   {
      private readonly QueryResultIterator<TInfluxRow> _resultIterator;
      private InfluxResult<TInfluxRow> _currentResult;
      private bool? _consumedNextResult_Result;

      private InfluxSeries<TInfluxRow> _currentSerie;
      private bool? _consumedNextSerie_Result;

      private bool hasConsumedCurrentBatch;
      private bool _disposed = false;

      public ContextualQueryResultIterator( QueryResultIterator<TInfluxRow> resultIterator )
      {
         _resultIterator = resultIterator;
      }

      public async Task<bool> ConsumeNextResultAsync()
      {
         if( _consumedNextResult_Result.HasValue )
         {
            var resultOfPreviousCall = _consumedNextResult_Result.Value;
            _consumedNextResult_Result = null;
            return resultOfPreviousCall;
         }

         bool hasMore = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
         if( !hasMore )
         {
            _currentResult = null;
            return false;
         }

         _currentResult = _resultIterator.CurrentResult;

         return true;
      }

      public async Task<bool> ConsumeNextSerieAsync()
      {
         if( _consumedNextSerie_Result.HasValue )
         {
            var resultOfPreviousCall = _consumedNextSerie_Result.Value;
            _consumedNextSerie_Result = null;
            return resultOfPreviousCall;
         }

         bool hasMore = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
         if( !hasMore ) // 'next' serie might be in next result, which means we should still return true
         {
            // in which case we might consume a result, we might not use here
            _consumedNextResult_Result = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
            if( _consumedNextResult_Result == true ) // may be consumed!!!
            {
               var previousResult = _currentResult;
               _currentResult = _resultIterator.CurrentResult;

               if( previousResult.StatementId == _currentResult.StatementId )
               {
                  hasMore = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
                  if( hasMore )
                  {
                     _currentSerie = _resultIterator.CurrentSerie;

                     return true;
                  }

                  _consumedNextResult_Result = null; // still the same statement, so we consumed it
               }
            }
            else
            {
               _currentResult = null;
            }


            return false;
         }

         _currentSerie = _resultIterator.CurrentSerie;

         return true;
      }

      public async Task<List<TInfluxRow>> GetNextBatchAsync()
      {
         if( hasConsumedCurrentBatch )
         {
            var previousSerie = _currentSerie;
            _consumedNextSerie_Result = await ConsumeNextSerieAsync().ConfigureAwait( false ); 
            if( !_consumedNextSerie_Result.Value )
            {
               hasConsumedCurrentBatch = false;
               return null;
            }

            if( previousSerie == null || InfluxSeriesComparer.Compare( previousSerie, _currentSerie ) )
            {
               _consumedNextSerie_Result = null;
               hasConsumedCurrentBatch = true;
               return _currentSerie.Rows;
            }

            // might return null
            hasConsumedCurrentBatch = false;
            return null;
         }
         else
         {
            hasConsumedCurrentBatch = true;
            return _currentSerie.Rows;
         }

         // need to consume to next serie, and check if it is the same serie in the same statement(?)
      }

      public InfluxResult<TInfluxRow> CurrentResult
      {
         get
         {
            return _currentResult;
         }
      }

      public InfluxSeries<TInfluxRow> CurrentSerie
      {
         get
         {
            return _currentSerie;
         }
      }

      protected virtual void Dispose( bool disposing )
      {
         if( !_disposed )
         {
            if( disposing )
            {
               _resultIterator.Dispose();
            }

            _disposed = true;
         }
      }

      // This code added to correctly implement the disposable pattern.
      public void Dispose()
      {
         Dispose( true );
      }
   }
}
