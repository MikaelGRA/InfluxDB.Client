using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public sealed class InfluxQueryOptions
   {
      public InfluxQueryOptions()
      {
         Precision = TimestampPrecision.Nanosecond;
      }

      public TimestampPrecision Precision { get; set; }
   }
}
