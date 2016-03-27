using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class RowProjection
   {
      public RowProjection()
      {
         Bindings = new List<ColumnBinding>();
      }
      
      public RowProjection InnerProjection { get; set; }

      public LambdaExpression Projector { get; set; }

      public List<ColumnBinding> Bindings { get; set; }
   }
}
