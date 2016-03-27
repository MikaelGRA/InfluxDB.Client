using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class ColumnBinding
   {
      public Expression Source { get; set; }

      public MemberInfo Target { get; set; }
   }
}
