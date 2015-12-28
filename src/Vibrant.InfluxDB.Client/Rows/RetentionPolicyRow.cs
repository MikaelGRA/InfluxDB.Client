using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class RetentionPolicyRow
   {
      [InfluxField("name")]
      public string Name { get; set; }

      [InfluxField( "duration" )]
      public string Duration { get; set; }

      [InfluxField( "replicaN" )]
      public long Replication { get; set; }

      [InfluxField( "default" )]
      public bool Default { get; set; }
   }
}
