using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   internal class DateTimeRow
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxField( "otherTime" )]
      internal DateTime OtherTime { get; set; }

      [InfluxField( "otherTimeAsString" )]
      internal string OtherTimeAsString { get; set; }
   }
}
