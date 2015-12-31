using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW SHARDS query.
   /// </summary>
   public class ShardRow
   {
      /// <summary>
      /// Gets the ID of the shard.
      /// </summary>
      [InfluxField( "id" )]
      public long Id { get; private set; }

      /// <summary>
      /// Gets the database.
      /// </summary>
      [InfluxField( "database" )]
      public string Database { get; private set; }

      /// <summary>
      /// Gets the retention policy.
      /// </summary>
      [InfluxField( "retention_policy" )]
      public string RetentionPolicy { get; private set; }

      /// <summary>
      /// Gets the shard group.
      /// </summary>
      [InfluxField( "shard_group" )]
      public long ShardGroup { get; private set; }

      /// <summary>
      /// Gets the start time.
      /// </summary>
      [InfluxField( "start_time" )]
      public DateTime StartTime { get; private set; }
      
      /// <summary>
      /// Gets the end time.
      /// </summary>
      [InfluxField( "end_time" )]
      public DateTime EndTime { get; private set; }

      /// <summary>
      /// Gets the expiry time.
      /// </summary>
      [InfluxField( "expiry_time" )]
      public DateTime ExpiryTime { get; private set; }

      /// <summary>
      /// Gets the owners.
      /// </summary>
      [InfluxField( "owners" )]
      public long Owners { get; private set; }
   }
}
