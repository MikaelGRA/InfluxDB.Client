//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Linq;
//using Vibrant.InfluxDB.Client.Metadata;
//using Vibrant.InfluxDB.Client.Parsers;

//namespace Vibrant.InfluxDB.Client.Visitors
//{
//   public class OrderByClauseGenerator<TInfluxRow> : ProjectingClauseGenerator<TInfluxRow>
//      where TInfluxRow : new()
//   {
//      private OrderByClause _orderBy;

//      internal string GetOrderByClause( OrderByClause orderBy )
//      {
//         Projection = orderBy.Projection;
//         _orderBy = orderBy;

//         Visit( _orderBy.Expression );

//         Clause.Remove( Clause.Length - 2, 2 );

//         if( orderBy.IsAscending )
//         {
//            Clause.Append( " ASC" );
//         }
//         else
//         {
//            Clause.Append( " DESC" );
//         }

//         return Clause.ToString();
//      }

//      protected override void OnMemberFound( MemberInfo member )
//      {
//         var property = Metadata.PropertiesByClrName[ member.Name ];

//         Clause.Append( property.QueryProtocolEscapedKey )
//            .Append( ", " );
//      }

//      protected override Expression VisitMethodCall( MethodCallExpression node )
//      {
//         if( node.Method.DeclaringType == typeof( InfluxFunctions ) )
//         {
//            if( node.Method.Name == "Count" )
//            {
//               Clause.Append( "COUNT(" );
//               Visit( node.Arguments[ 0 ] );
//               Clause.Append( ")" );
//            }

//            return node;
//         }
//         else
//         {
//            throw new NotSupportedException( $"The method '{node.Method.Name}' on '{node.Method.DeclaringType.FullName}' is not supported." );
//         }
//      }

//      protected override Expression VisitBinary( BinaryExpression b )
//      {
//         throw new NotSupportedException( $"The binary operator '{b.NodeType}' is not supported" );
//      }

//      protected override Expression VisitConstant( ConstantExpression node )
//      {
//         throw new NotSupportedException( $"Constants are not supported in the order by clause." );
//      }
//   }
//}
