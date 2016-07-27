using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
    public class ShowSeriesRow
   {
      [InfluxField( "key" )]
      public string Key { get; private set; }
   }
}
