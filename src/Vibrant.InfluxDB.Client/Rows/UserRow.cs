using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class UserRow
   {
      [InfluxField( "user" )]
      public string Username { get; set; }

      [InfluxField( "admin" )]
      public bool IsAdmin { get; set; }
   }
}
