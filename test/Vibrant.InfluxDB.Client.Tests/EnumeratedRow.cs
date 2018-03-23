using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class EnumeratedRow
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxField( "value" )]
      public double Value { get; set; }

      [InfluxTag( "type" )]
      public TestEnum1? Type { get; set; }

      [InfluxTag( "intType" )]
      public int IntType { get; set; }
   }
}
