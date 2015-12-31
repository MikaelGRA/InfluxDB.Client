using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Interface that can be used to specify a per-row measurement name.
   /// </summary>
   public interface IHaveMeasurementName
   {
      /// <summary>
      /// Gets or sets the measurement name.
      /// </summary>
      string MeasurementName { get; set; }
   }
}
