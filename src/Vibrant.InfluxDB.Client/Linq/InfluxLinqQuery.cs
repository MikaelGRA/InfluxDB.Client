using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Linq
{
   internal class InfluxLinqQuery<TInfluxRow>
   {
      internal InfluxLinqQuery()
      {
         Selects = new List<FieldSelect<TInfluxRow>>();
      }

      internal List<FieldSelect<TInfluxRow>> Selects { get; set; }

      internal string Where { get; set; }

      internal string GroupBy { get; set; }
   }
}
