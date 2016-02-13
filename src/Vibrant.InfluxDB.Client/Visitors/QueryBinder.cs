using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Linq;

namespace Vibrant.InfluxDB.Client.Visitors
{
   internal class QueryBinder : ExpressionVisitor
   {
      internal QueryBinder()
      {

      }

      internal string Translate( Expression expression )
      {
         Visit( expression );

         return "";
      }

      protected override Expression VisitConstant( ConstantExpression node )
      {
         return base.VisitConstant( node );
      }

      protected override Expression VisitBinary( BinaryExpression node )
      {
         return base.VisitBinary( node );
      }

      protected override Expression VisitMember( MemberExpression node )
      {
         return base.VisitMember( node );
      }

      protected override Expression VisitMethodCall( MethodCallExpression node )
      {
         return base.VisitMethodCall( node );
      }

      protected override Expression VisitParameter( ParameterExpression node )
      {
         return base.VisitParameter( node );
      }

      protected override Expression VisitUnary( UnaryExpression node )
      {
         return base.VisitUnary( node );
      }
   }
}
