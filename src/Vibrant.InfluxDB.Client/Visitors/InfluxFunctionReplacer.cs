//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;

//namespace Vibrant.InfluxDB.Client.Visitors
//{
//   public class InfluxFunctionReplacer : ExpressionVisitor
//   {
//      public static Expression Replace( Expression expression )
//      {
//         return new InfluxFunctionReplacer().Visit( expression );
//      }

//      protected override Expression VisitMethodCall( MethodCallExpression node )
//      {
//         foreach( var arg in node.Arguments )
//         {
//            Visit( arg );
//         }

//         // PROBLEM: Cannot late convert types, because they have already been stored in incompatible data types

//         if( node.Method.DeclaringType == typeof( InfluxFunctions ) )
//         {
//            // simply replace the method call with the parameter
//            return Expression.Convert( node.Arguments[ 0 ], node.Type );
//         }
//         return node;
//      }
//   }
//}
