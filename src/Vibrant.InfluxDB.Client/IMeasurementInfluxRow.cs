using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public interface IMeasurementInfluxRow : IInfluxRow
   {
      string SeriesName { get; set; }
   }
}
