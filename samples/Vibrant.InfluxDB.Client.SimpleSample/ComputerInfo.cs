using System;
using System.Collections.Generic;
using System.Text;

namespace Vibrant.InfluxDB.Client.SimpleSample
{
   public class ComputerInfo
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxTag( "host" )]
      public string Host { get; set; }

      [InfluxTag( "region" )]
      public string Region { get; set; }

      [InfluxField( "cpu" )]
      public double CPU { get; set; }

      [InfluxField( "ram" )]
      public long RAM { get; set; }
   }
}
