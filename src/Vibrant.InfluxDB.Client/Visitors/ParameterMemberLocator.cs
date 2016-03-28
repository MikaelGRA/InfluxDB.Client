using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   internal class ParameterMemberLocator : ExpressionVisitor
   {
      private MemberInfo _locatedMember;

      internal static MemberInfo Locate( Expression source )
      {
         var locator = new ParameterMemberLocator();
         locator.Visit( source );

         if( locator._locatedMember == null )
         {
            throw new NotSupportedException();
         }

         return locator._locatedMember;
      }

      protected override Expression VisitMember( MemberExpression node )
      {
         if( node.Expression.NodeType == ExpressionType.Parameter )
         {
            _locatedMember = node.Member;
         }
         return node;
      }
   }
}
