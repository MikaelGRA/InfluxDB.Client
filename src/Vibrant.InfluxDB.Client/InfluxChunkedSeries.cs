using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
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
      public async Task<InfluxChunk<TInfluxRow>> GetNextChunkAsync()
      {
         List<TInfluxRow> batch;
         if( ( batch = await _iterator.GetNextBatchAsync().ConfigureAwait( false ) ) != null )
         {
            return new InfluxChunk<TInfluxRow>( batch );
         }

         return null;
      }
   }
}
