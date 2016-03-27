using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class SelectClause
   {
      public SelectClause( RowProjection projection )
      {
         Projection = projection;
      }

      public RowProjection Projection { get; private set; }
   }
}
