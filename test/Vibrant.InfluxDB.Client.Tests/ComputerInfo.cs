using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class ComputerInfo
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxTag( "host" )]
      internal string Host { get; set; }

      [InfluxTag( "region" )]
      internal string Region { get; set; }

      [InfluxField( "cpu" )]
      internal double? CPU { get; set; }

      [InfluxField( "ram" )]
      internal long RAM { get; set; }
   }
   public class ComputedComputerInfo
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxComputed( "cpu" )]
      internal double? CPU { get; set; }

      [InfluxComputed( "ram" )]
      internal long RAM { get; set; }
   }

   public class ComputerInfoMeta
   {
      [InfluxField( "host" )]
      internal string Host { get; set; }

      [InfluxField( "region" )]
      internal string Region { get; set; }
   }
}
