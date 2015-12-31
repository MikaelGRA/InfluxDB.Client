using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class DateTimeRow
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxField( "otherTime" )]
      public DateTime OtherTime { get; set; }

      [InfluxField( "otherTimeAsString" )]
      public string OtherTimeAsString { get; set; }
   }
}
