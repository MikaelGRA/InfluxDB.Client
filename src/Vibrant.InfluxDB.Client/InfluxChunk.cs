using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunk<TInfluxRow>
   {
      internal InfluxChunk( List<TInfluxRow> rows )
      {
         Rows = rows;
      }

      /// <summary>
      /// Gets the rows of the InfluxChunk.
      /// </summary>
      public List<TInfluxRow> Rows { get; private set; }
   }
}
