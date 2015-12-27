using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class ContinuousQueryRow
   {
      [InfluxField( "name" )]
      public string Name { get; set; }

      [InfluxField( "query" )]
      public string Query { get; set; }
   }
}
