using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public interface IHaveMeasurementName
   {
      string MeasurementName { get; set; }
   }
}
