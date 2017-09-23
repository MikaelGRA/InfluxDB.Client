using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing the structured returned by the SHOW SERIES command.
   /// </summary>
   public class ShowSeriesRow
   {
      /// <summary>
      /// Gets the key.
      /// </summary>
      [InfluxField( "key" )]
      public string Key { get; private set; }
   }
}
