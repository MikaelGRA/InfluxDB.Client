using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public sealed class InfluxWriteOptions
   {
      public InfluxWriteOptions()
      {
         Consistency = Consistency.All;
         Precision = TimestampPrecision.Nanosecond;
      }

      public Consistency Consistency { get; set; }

      public TimestampPrecision Precision { get; set; }
   }
}
