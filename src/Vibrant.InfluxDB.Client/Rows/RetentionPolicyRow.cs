using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW RETENTION POLICIES query.
   /// </summary>
   public class RetentionPolicyRow
   {
      /// <summary>
      /// Gets the name of the retention policy.
      /// </summary>
      [InfluxField("name")]
      public string Name { get; private set; }

      /// <summary>
      /// Gets the duration of rows using this retention policy.
      /// </summary>
      [InfluxField( "duration" )]
      public string Duration { get; private set; }

      /// <summary>
      /// Gets the number of replicas of rows using this retention policy.
      /// </summary>
      [InfluxField( "replicaN" )]
      public long Replication { get; private set; }

      /// <summary>
      /// Gets an indication of whether or not this is the default retention policy
      /// for the database.
      /// </summary>
      [InfluxField( "default" )]
      public bool Default { get; private set; }
   }
}
