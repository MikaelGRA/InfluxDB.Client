using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
    public enum TimestampPrecision
    {
      Nanosecond,
      Microsecond,
      Millisecond,
      Second,
      Minute,
      Hours
    }
}
