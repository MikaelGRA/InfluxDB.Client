using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client.Linq
{
   internal class FieldSelect<TInfluxRow>
   {
      internal FieldSelect( PropertyExpressionInfo<TInfluxRow> field )
      {
         Field = field;
      }

      internal PropertyExpressionInfo<TInfluxRow> Field { get; set; }
   }
}
