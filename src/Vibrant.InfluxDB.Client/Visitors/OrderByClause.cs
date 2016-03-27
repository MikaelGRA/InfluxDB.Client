using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class OrderByClause
   {
      public OrderByClause( bool isAscending, Expression expression, RowProjection projection )
      {
         IsAscending = isAscending;
         Expression = expression;
         Projection = projection;
      }

      public bool IsAscending { get; private set; }

      public Expression Expression { get; private set; }

      public RowProjection Projection { get; private set; }
   }
}
