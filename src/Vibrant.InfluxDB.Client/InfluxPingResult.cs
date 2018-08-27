using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Class representing the result of a ping to InfluxDB.
   /// </summary>
   public class InfluxPingResult
   {
      /// <summary>
      /// Gets the version of InfluxDB.
      /// </summary>
      public string Version { get; set; }
   }
}
