using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Enum representing the database privileges of InfluxDB.
   /// </summary>
   public enum DatabasePriviledge
   {
      /// <summary>
      /// No access to the database.
      /// </summary>
      [EnumMember( Value = "NO PRIVILEGES" )]
      None        = 0x00,

      /// <summary>
      /// Read access to the database.
      /// </summary>
      [EnumMember( Value = "READ" )]
      Read        = 0x01,

      /// <summary>
      /// Write access to the database.
      /// </summary>
      [EnumMember( Value = "WRITE" )]
      Write       = 0x02,

      /// <summary>
      /// Full access to the database.
      /// </summary>
      [EnumMember( Value = "ALL PRIVILEGES" )]
      All         = 0x04
   }
}
