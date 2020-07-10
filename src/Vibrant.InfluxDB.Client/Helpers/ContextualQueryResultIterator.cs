using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal class ContextualQueryResultIterator<TInfluxRow> : IDisposable
      where TInfluxRow : new()
   {
      private readonly QueryResultIterator<TInfluxRow> _resultIterator;

      private InfluxResult<TInfluxRow> _currentResult;
      private InfluxResult<TInfluxRow> _capturedResult;
      private int? _currentStatementId;
      private bool _consumeCurrentResultNextTime;

      private InfluxSeries<TInfluxRow> _currentSerie;
      private InfluxSeries<TInfluxRow> _capturedSerie;
      private IReadOnlyDictionary<string, object> _currentTags;
      private bool _consumeCurrentSerieNextTime;

      private List<TInfluxRow> _currentBatch;
      private List<TInfluxRow> _capturedBatch;

      private bool _disposed = false;

      public ContextualQueryResultIterator( QueryResultIterator<TInfluxRow> resultIterator )
      {
         _resultIterator = resultIterator;
      }


      public async Task<bool> ConsumeNextResultAsync(CancellationToken cancellationToken = default)
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

            hasMore = await _resultIterator.ConsumeNextResultAsync(cancellationToken).ConfigureAwait( false );
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

      public async Task<bool> ConsumeNextSerieAsync(CancellationToken cancellationToken = default)
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
               hasMoreSeries = await _resultIterator.ConsumeNextSerieAsync(cancellationToken).ConfigureAwait( false );
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
            var hasMoreResults = await _resultIterator.ConsumeNextResultAsync(cancellationToken).ConfigureAwait( false );
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

      public async Task<bool> ConsumeNextBatchAsync(CancellationToken cancellationToken = default)
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
                  hasMoreSeries = await _resultIterator.ConsumeNextSerieAsync(cancellationToken).ConfigureAwait( false );
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
               var hasMoreResults = await _resultIterator.ConsumeNextResultAsync(cancellationToken).ConfigureAwait( false );
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

      public InfluxResult<TInfluxRow> CurrentResult => _capturedResult;

      public InfluxSeries<TInfluxRow> CurrentSerie => _capturedSerie;

      public List<TInfluxRow> CurrentBatch => _capturedBatch;

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
