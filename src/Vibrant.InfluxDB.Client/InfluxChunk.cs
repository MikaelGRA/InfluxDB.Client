using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunk<TInfluxRow>
   {
      public InfluxChunk()
      {
         Rows = new List<TInfluxRow>();
      }

      /// <summary>
      /// Gets the rows of the InfluxChunk.
      /// </summary>
      public List<TInfluxRow> Rows { get; set; }
   }
}
