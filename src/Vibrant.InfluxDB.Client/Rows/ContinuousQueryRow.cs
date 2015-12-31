using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW CONTINUOUS QUERIES.
   /// </summary>
   public class ContinuousQueryRow
   {
      /// <summary>
      /// Gets the name.
      /// </summary>
      [InfluxField( "name" )]
      public string Name { get; private set; }

      /// <summary>
      /// Gets the query.
      /// </summary>
      [InfluxField( "query" )]
      public string Query { get; private set; }
   }
}
