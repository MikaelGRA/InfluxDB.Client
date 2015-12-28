using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class GrantsRow
   {
      [InfluxField( "database" )]
      public string Database { get; set; }

      [InfluxField( "privilege" )]
      public DatabasePriviledge Privilege { get; set; }
   }
}
