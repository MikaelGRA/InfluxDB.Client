using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW MEASUREMENTS query.
   /// </summary>
   public class MeasurementRow
   {
      /// <summary>
      /// Gets the name of the measurement.
      /// </summary>
      [InfluxField( "name" )]
      public string Name { get; private set; }
   }
}
