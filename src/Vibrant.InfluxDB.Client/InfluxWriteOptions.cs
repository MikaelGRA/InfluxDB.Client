using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// The write options that are being used when write operations are performed against influxDB.
   /// </summary>
   public sealed class InfluxWriteOptions
   {
      /// <summary>
      /// Constructs a new InfluxWriteOptions with default values.
      /// </summary>
      public InfluxWriteOptions()
      {
         Consistency = Consistency.All;
         Precision = TimestampPrecision.Nanosecond;
         UseGzip = false;
      }

      /// <summary>
      /// Gets or sets the consistency. Default is All.
      /// </summary>
      public Consistency Consistency { get; set; }

      /// <summary>
      /// Gets or sets the precision. Default is nanosecond.
      /// </summary>
      public TimestampPrecision Precision { get; set; }

      /// <summary>
      /// Gets or sets the retention policy to write. If omitted (null, default), writes go to database's default RP.
      /// </summary>
      public string RetentionPolicy { get; set; }

      /// <summary>
      /// Gets or sets a bool indicating if gzip compression should be used for write operations.
      /// </summary>
      public bool UseGzip { get; set; }
   }
}
