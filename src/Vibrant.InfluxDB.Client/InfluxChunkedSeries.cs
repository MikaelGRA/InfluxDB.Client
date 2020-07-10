using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// InfluxChunkedSeries represents a result when using the ReadChunkedAsync method of InfluxClient.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxChunkedSeries<TInfluxRow>
      where TInfluxRow : new()
   {
      private readonly ContextualQueryResultIterator<TInfluxRow> _iterator;

      internal InfluxChunkedSeries( ContextualQueryResultIterator<TInfluxRow> iterator, string name, IReadOnlyDictionary<string, object> tags )
      {
         _iterator = iterator;

         Name = name;
         GroupedTags = tags;
      }

      /// <summary>
      /// Gets the name of the measurement or series.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Gets the tags that this InfluxSeries has been grouped on.
      /// </summary>
      public IReadOnlyDictionary<string, object> GroupedTags { get; private set; }

      /// <summary>
      /// Gets the next chunk from the serie.
      /// 
      /// Null if none are available.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxChunk<TInfluxRow>> GetNextChunkAsync(CancellationToken cancellationToken = default)
      {
         if( await _iterator.ConsumeNextBatchAsync(cancellationToken).ConfigureAwait( false ) )
         {
            return new InfluxChunk<TInfluxRow>( _iterator.CurrentBatch );
         }

         return null;
      }
   }
}
