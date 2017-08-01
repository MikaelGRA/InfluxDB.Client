using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunkedSeries<TInfluxRow>
   {
      public InfluxChunkedSeries( string name, IDictionary<string, object> tags )
      {
         Name = name;
         if( tags != null )
         {
            GroupedTags = new ReadOnlyDictionary<string, object>( tags );
         }
      }

      /// <summary>
      /// Gets the name of the measurement or series.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Gets the tags that this InfluxSeries has been grouped on.
      /// </summary>
      public IReadOnlyDictionary<string, object> GroupedTags { get; private set; }

      public async Task<InfluxChunk<TInfluxRow>> GetNextChunkAsync()
      {
         throw new NotImplementedException();
      }
   }
}
