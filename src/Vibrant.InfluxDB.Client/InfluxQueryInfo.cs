using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Visitors;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxQueryInfo<TInfluxRow>
      where TInfluxRow : new()
   {
      public string Database { get; private set; }

      public string MeasurementName { get; private set; }

      public InfluxQueryInfo( string db, string measurementName )
      {
         Database = db;
         MeasurementName = measurementName;
      }

      public Expression Where { get; private set; }

      public void IncludeWhere( Expression where )
      {
         if( Where == null )
         {
            Where = where;
         }
         else
         {
            Where = Expression.AndAlso( Where, where );
         }
      }

      public string GenerateInfluxQL()
      {
         var sb = new StringBuilder();
         if( Where != null )
         {
            sb.Append( new WhereClauseGenerator<TInfluxRow>().GetWhereClause( Where ) );
         }
         return sb.ToString();
      }
   }
}
