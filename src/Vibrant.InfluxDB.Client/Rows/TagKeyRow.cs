using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
    public class TagKeyRow : IInfluxRow
    {
      [InfluxField( "tagKey" )]
      public string TagKey { get; set; }
   }
}
