using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW GRANTS.
   /// </summary>
   public class GrantsRow
   {
      /// <summary>
      /// Gets the database of the grant.
      /// </summary>
      [InfluxField( "database" )]
      public string Database { get; private set; }

      /// <summary>
      /// Gets the privilege of the grant.
      /// </summary>
      [InfluxField( "privilege" )]
      public DatabasePriviledge Privilege { get; private set; }
   }
}
