using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW USERS query.
   /// </summary>
   public class UserRow
   {
      /// <summary>
      /// Gets the username.
      /// </summary>
      [InfluxField( "user" )]
      public string Username { get; private set; }

      /// <summary>
      /// Gets an indication of whether the user is an admin.
      /// </summary>
      [InfluxField( "admin" )]
      public bool IsAdmin { get; private set; }
   }
}
