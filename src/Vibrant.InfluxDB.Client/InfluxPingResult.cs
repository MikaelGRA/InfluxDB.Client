using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxPingResult
   {
      internal InfluxPingResult()
      {

      }

      /// <summary>
      /// Gets the version of InfluxDB.
      /// </summary>
      public string Version { get; set; }
   }
}
