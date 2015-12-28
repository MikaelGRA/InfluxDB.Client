using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class VariationRow
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxTag( "type odd name" )]
      public string Type { get; set; }

      [InfluxTag( "categoryTag" )]
      public TestEnum2 CategoryTag { get; set; }

      [InfluxField( "message" )]
      public string Message { get; set; }

      [InfluxField( "count" )]
      public long Count { get; set; }

      [InfluxField( "percent fun name" )]
      public double Percent { get; set; }

      [InfluxField( "indicator" )]
      public bool Indicator { get; set; }

      [InfluxField( "category" )]
      public TestEnum1 Category { get; set; }
   }
}
