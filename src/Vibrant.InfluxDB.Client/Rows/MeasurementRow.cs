using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class MeasurementRow
   {
      [InfluxField( "name" )]
      public string Namem { get; set; }
   }
}
