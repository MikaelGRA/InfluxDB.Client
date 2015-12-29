using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class ShardRow
   {
      [InfluxField( "id" )]
      public long Id { get; set; }

      [InfluxField( "database" )]
      public string Database { get; set; }

      [InfluxField( "retention_policy" )]
      public string RetentionPolicy { get; set; }

      [InfluxField( "shard_group" )]
      public long ShardGroup { get; set; }

      [InfluxField( "start_time" )]
      public DateTime StartTime { get; set; }
      
      [InfluxField( "end_time" )]
      public DateTime EndTime { get; set; }

      [InfluxField( "expiry_time" )]
      public DateTime ExpiryTime { get; set; }

      [InfluxField( "owners" )]
      public long Owners { get; set; }
   }
}
