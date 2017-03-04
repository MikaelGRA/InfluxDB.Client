using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// InfluxQueryOptions are applied to queries made to InfluxDB.
   /// </summary>
   public sealed class InfluxQueryOptions
   {
      /// <summary>
      /// Constructs an InfluxQueryOptions with default values.
      /// </summary>
      public InfluxQueryOptions()
      {
         Precision = null;
         ChunkSize = null;
      }

      /// <summary>
      /// Gets or sets the Precision. Default is nanoseconds.
      /// </summary>
      public TimestampPrecision? Precision { get; set; }

      /// <summary>
      /// Gets or sets the ChunkSize. Default is null. Which means
      /// it uses the InfluxDB default of 10000.
      /// </summary>
      public int? ChunkSize { get; set; }
   }
}
