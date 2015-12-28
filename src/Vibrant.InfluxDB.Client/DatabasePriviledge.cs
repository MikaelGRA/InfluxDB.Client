using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public enum DatabasePriviledge
   {
      [EnumMember( Value = "NO PRIVILEGES" )]
      None        = 0x00,

      [EnumMember( Value = "READ" )]
      Read        = 0x01,

      [EnumMember( Value = "WRITE" )]
      Write       = 0x02,

      [EnumMember( Value = "ALL PRIVILEGES" )]
      All         = 0x04
   }
}
