using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class RegionRow
   {
      [InfluxField( "region" )]
      public string Region { get; set; }
   }
}
