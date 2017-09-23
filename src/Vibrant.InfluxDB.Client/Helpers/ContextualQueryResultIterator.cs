using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal enum AllowedCalls
   {
      ConsumeNextResultAsync = 0b001,
      ConsumeNextSerieAsync = 0b010,
      GetNextBatchAsync = 0b100
   }

   internal class ContextualQueryResultIterator<TInfluxRow> : IDisposable
      where TInfluxRow : new()
   {
      private readonly QueryResultIterator<TInfluxRow> _resultIterator;

      private InfluxResult<TInfluxRow> _currentResult;
      private InfluxResult<TInfluxRow> _capturedResult;
      private int? _currentStatementId;
      private bool _consumeCurrentResultNextTime;

      //private bool? _consumedNextResult_Result;

      private InfluxSeries<TInfluxRow> _currentSerie;
      private InfluxSeries<TInfluxRow> _capturedSerie;
      private IReadOnlyDictionary<string, object> _currentTags;
      private bool _consumeCurrentSerieNextTime;

      //private bool? _consumedNextSerie_Result;

      private List<TInfluxRow> _currentBatch;
      private List<TInfluxRow> _capturedBatch;
      //private bool _hasConsumedCurrentBatch = true;

      private bool _disposed = false;

      public ContextualQueryResultIterator( QueryResultIterator<TInfluxRow> resultIterator )
      {
         _resultIterator = resultIterator;
      }


      public async Task<bool> ConsumeNextResultAsync()
      {
         if( _consumeCurrentResultNextTime )
         {
            _consumeCurrentResultNextTime = false;
            if( _currentStatementId != _currentResult.StatementId )
            {
               _currentStatementId = _currentResult.StatementId;
               _capturedResult = _currentResult;
               return true;
            }
         }

         bool hasMore;
         do
         {

            hasMore = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
            if( hasMore )
            {
               _currentResult = _resultIterator.CurrentResult;
               if( _currentResult.StatementId != _currentStatementId )
               {
                  _currentStatementId = _currentResult.StatementId;
                  _capturedResult = _currentResult;
                  return true;
               }
            }

         } while( hasMore );

         _currentStatementId = null;
         _capturedResult = null;
         return false;
      }

      public async Task<bool> ConsumeNextSerieAsync()
      {
         if( _consumeCurrentSerieNextTime )
         {
            _consumeCurrentSerieNextTime = false;

            if( _currentStatementId != _currentResult.StatementId )
            {
               _consumeCurrentResultNextTime = true;
               _currentTags = null;
               _capturedSerie = null;
               return false;
            }

            if( !InfluxSeriesComparer.Compare( _currentSerie.GroupedTags, _currentTags ) )
            {
               _currentTags = _currentSerie.GroupedTags;
               _capturedSerie = _currentSerie;
               _currentBatch = _currentSerie.Rows;
               return true;
            }
         }

         bool hasMoreSeries;
         bool hasMoreResultsForCurrentStatement = true;

         while( hasMoreResultsForCurrentStatement ) // hasMoreResults
         {
            do
            {
               // might need to consume both results and series
               hasMoreSeries = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
               if( hasMoreSeries )
               {
                  // consume until the grouped tags changes
                  _currentSerie = _resultIterator.CurrentSerie;
                  if( !InfluxSeriesComparer.Compare( _currentSerie.GroupedTags, _currentTags ) )
                  {
                     _currentTags = _currentSerie.GroupedTags;
                     _currentBatch = _currentSerie.Rows;
                     _capturedSerie = _currentSerie;
                     return true;
                  }
               }

            } while( hasMoreSeries );


            // handle the next result to see if it contains more data for the current statement
            var hasMoreResults = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
            if( hasMoreResults )
            {
               _currentResult = _resultIterator.CurrentResult;

               hasMoreResultsForCurrentStatement = _currentResult.StatementId == _currentStatementId;

               if( !hasMoreResultsForCurrentStatement )
               {
                  _consumeCurrentResultNextTime = true;
               }
            }
            else
            {
               hasMoreResultsForCurrentStatement = false;
            }
         }

         _capturedSerie = null;
         _currentTags = null;
         return false;
      }

      public async Task<bool> ConsumeNextBatchAsync()
      {
         if( _currentBatch != null )
         {
            _capturedBatch = _currentBatch;
            _currentBatch = null;
            return true;
         }
         else
         {
            bool hasMoreSeries;
            bool hasMoreResultsForCurrentStatement = true;

            while( hasMoreResultsForCurrentStatement ) // hasMoreResults
            {
               do
               {
                  // might need to consume both results and series
                  hasMoreSeries = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
                  if( hasMoreSeries )
                  {
                     // consume until the grouped tags changes
                     _currentSerie = _resultIterator.CurrentSerie;
                     if( InfluxSeriesComparer.Compare( _currentSerie.GroupedTags, _currentTags ) )
                     {
                        _capturedBatch = _currentSerie.Rows;
                        return true;
                     }
                     else
                     {
                        _consumeCurrentSerieNextTime = true;
                        _currentBatch = null;
                        _capturedBatch = null;
                        return false;
                     }

                  }

               } while( hasMoreSeries );


               // handle the next result to see if it contains more data for the current statement
               var hasMoreResults = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
               if( hasMoreResults )
               {
                  _currentResult = _resultIterator.CurrentResult;

                  hasMoreResultsForCurrentStatement = _currentResult.StatementId == _currentStatementId;

                  if( !hasMoreResultsForCurrentStatement )
                  {
                     _consumeCurrentSerieNextTime = true;
                  }
               }
               else
               {
                  hasMoreResultsForCurrentStatement = false;
               }
            }

            _capturedBatch = null;
            return false;
         }
      }



      //public async Task<bool> ConsumeNextResultAsync()
      //{
      //   // throw invalid operation if not called correctly!

      //   if( _consumedNextResult_Result.HasValue )
      //   {
      //      var resultOfPreviousCall = _consumedNextResult_Result.Value;
      //      _consumedNextResult_Result = null;
      //      return resultOfPreviousCall;
      //   }

      //   bool hasMore = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
      //   if( !hasMore )
      //   {
      //      _currentResult = null;
      //      return false;
      //   }

      //   _currentResult = _resultIterator.CurrentResult;
      //   return true;
      //}


      //public async Task<bool> ConsumeNextSerieAsync()
      //{
      //   // throw invalid operation if not called correctly!

      //   if( _consumedNextSerie_Result.HasValue )
      //   {
      //      var resultOfPreviousCall = _consumedNextSerie_Result.Value;
      //      _consumedNextSerie_Result = null;
      //      return resultOfPreviousCall;
      //   }

      //   bool hasMore = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
      //   if( !hasMore ) // 'next' serie might be in next result, which means we should still return true
      //   {
      //      // in which case we might consume a result, we might not use here
      //      _consumedNextResult_Result = await _resultIterator.ConsumeNextResultAsync().ConfigureAwait( false );
      //      if( _consumedNextResult_Result == true )
      //      {
      //         var previousResult = _currentResult;
      //         _currentResult = _resultIterator.CurrentResult;

      //         if( previousResult.StatementId == _currentResult.StatementId )
      //         {
      //            hasMore = await _resultIterator.ConsumeNextSerieAsync().ConfigureAwait( false );
      //            if( hasMore )
      //            {
      //               _currentSerie = _resultIterator.CurrentSerie;
      //               return true;
      //            }

      //            _consumedNextResult_Result = null; // still the same statement, so we consumed it
      //         }
      //      }
      //      else
      //      {
      //         _currentResult = null;
      //      }

      //      _currentSerie = null;
      //      return false;
      //   }

      //   _currentSerie = _resultIterator.CurrentSerie;
      //   return true;
      //}

      //public async Task<List<TInfluxRow>> GetNextBatchAsync()
      //{
      //   // throw invalid operation if not called correctly!

      //   if( hasConsumedCurrentBatch )
      //   {
      //      var previousSerie = _currentSerie;
      //      _consumedNextSerie_Result = await ConsumeNextSerieAsync().ConfigureAwait( false );
      //      if( !_consumedNextSerie_Result.Value )
      //      {
      //         hasConsumedCurrentBatch = false;
      //         return null;
      //      }

      //      if( previousSerie == null || InfluxSeriesComparer.Compare( previousSerie, _currentSerie ) )
      //      {
      //         _consumedNextSerie_Result = null;
      //         hasConsumedCurrentBatch = true;
      //         return _currentSerie.Rows;
      //      }

      //      // might return null
      //      hasConsumedCurrentBatch = false;
      //      return null;
      //   }
      //   else
      //   {
      //      hasConsumedCurrentBatch = true;
      //      return _currentSerie.Rows;
      //   }

      //   // need to consume to next serie, and check if it is the same serie in the same statement(?)
      //}

      public InfluxResult<TInfluxRow> CurrentResult
      {
         get
         {
            return _capturedResult;
         }
      }

      public InfluxSeries<TInfluxRow> CurrentSerie
      {
         get
         {
            return _capturedSerie;
         }
      }

      public List<TInfluxRow> CurrentBatch
      {
         get
         {
            return _capturedBatch;
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
