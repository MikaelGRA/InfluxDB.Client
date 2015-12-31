using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW DATABASES query.
   /// </summary>
   public class DatabaseRow
   {
      /// <summary>
      /// Gets or sets the name of the database.
      /// </summary>
      [InfluxField( "name" )]
      public string Name { get; private set; }
   }
}
