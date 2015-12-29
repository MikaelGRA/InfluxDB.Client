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
      public InfluxWriteOptions()
      {
         Consistency = Consistency.All;
         Precision = TimestampPrecision.Nanosecond;
      }

      /// <summary>
      /// Gets or sets the consistency. Default is All.
      /// </summary>
      public Consistency Consistency { get; set; }

      /// <summary>
      /// Gets or sets the precision. Default is nanosecond.
      /// </summary>
      public TimestampPrecision Precision { get; set; }
   }
}
