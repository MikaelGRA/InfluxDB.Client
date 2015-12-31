using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   internal class SimpleRow
   {
      [InfluxTimestamp]
      internal DateTime? Timestamp { get; set; }

      [InfluxField( "value" )]
      internal double Value { get; set; }
   }
}
