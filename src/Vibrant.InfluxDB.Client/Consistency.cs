using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Set the number of nodes that must confirm the write. If the requirement is not met the return value will be partial write if some points in the batch fail, or write failure if all points in the batch fail.
   /// </summary>
   public enum Consistency
    {
      /// <summary>
      /// The data must be written to disk by at least 1 valid node
      /// </summary>
      One,

      /// <summary>
      /// The data must be written to disk by (N/2 + 1) valid nodes (N is the replication factor for the target retention policy)
      /// </summary>
      Quorum,

      /// <summary>
      /// The data must be written to disk by all valid nodes
      /// </summary>
      All,

      /// <summary>
      /// a write is confirmed if hinted-handoff is successful, even if all target nodes report a write failure In this context, “valid node” means a node that hosts a copy of the shard containing the series being written to. In a clustered system, the replication factor should equal the number of valid nodes.
      /// </summary>
      Any
   }
}
