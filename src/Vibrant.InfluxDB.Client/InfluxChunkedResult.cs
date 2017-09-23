using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// InfluxChunkedResult represents a result when using the ReadChunkedAsync method of InfluxClient.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxChunkedResult<TInfluxRow>
      where TInfluxRow : new()
   {
      private readonly ContextualQueryResultIterator<TInfluxRow> _iterator;

      internal InfluxChunkedResult( ContextualQueryResultIterator<TInfluxRow> iterator, int statementId, string error )
      {
         _iterator = iterator;

         StatementId = statementId;
         ErrorMessage = error;
         Succeeded = error == null;
      }

      /// <summary>
      /// Gets the next serie from the result.
      /// 
      /// Null if none are available.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxChunkedSeries<TInfluxRow>> GetNextSeriesAsync()
      {
         if( await _iterator.ConsumeNextSerieAsync().ConfigureAwait( false ) )
         {
            var currentSerie = _iterator.CurrentSerie;

            return new InfluxChunkedSeries<TInfluxRow>(
               _iterator,
               currentSerie.Name,
               currentSerie.GroupedTags );
         }
         return null;
      }

      /// <summary>
      /// Gets the error message, if the operation did not succeed.
      /// </summary>
      public string ErrorMessage { get; private set; }

      /// <summary>
      /// Gets an indication of whether the operation succeeded.
      /// </summary>
      public bool Succeeded { get; private set; }

      /// <summary>
      /// Gets or sets the statement id.
      /// </summary>
      public int StatementId { get; private set; }
   }
}
