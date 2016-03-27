using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Linq;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class WhereClauseGenerator<TInfluxRow> : ExpressionVisitor
      where TInfluxRow : new()
   {
      private static readonly InfluxRowTypeInfo<TInfluxRow> Metadata = MetadataCache.GetOrCreate<TInfluxRow>();

      private StringBuilder _sb;

      internal string GetWhereClause( Expression expression )
      {
         _sb = new StringBuilder();
         _sb.Append( "WHERE " );
         Visit( expression );
         return _sb.ToString();
      }

      protected override Expression VisitBinary( BinaryExpression b )
      {
         _sb.Append( "(" );

         // TODO: Support ~=, !~

         Visit( b.Left );

         switch( b.NodeType )
         {
            case ExpressionType.Add:
               _sb.Append( " + " );
               break;
            case ExpressionType.Subtract:
               _sb.Append( " - " );
               break;
            case ExpressionType.Multiply:
               _sb.Append( " * " );
               break;
            case ExpressionType.Divide:
               _sb.Append( " / " );
               break;
            case ExpressionType.AndAlso:
               _sb.Append( " AND " );
               break;
            case ExpressionType.OrElse:
               _sb.Append( " OR " );
               break;
            case ExpressionType.Equal:
               _sb.Append( " = " );
               break;
            case ExpressionType.NotEqual:
               _sb.Append( " <> " );
               break;
            case ExpressionType.LessThan:
               _sb.Append( " < " );
               break;
            case ExpressionType.LessThanOrEqual:
               _sb.Append( " <= " );
               break;
            case ExpressionType.GreaterThan:
               _sb.Append( " > " );
               break;
            case ExpressionType.GreaterThanOrEqual:
               _sb.Append( " >= " );
               break;
            default:
               throw new NotSupportedException( $"The binary operator '{b.NodeType}' is not supported" );
         }

         Visit( b.Right );

         _sb.Append( ")" );

         return b;
      }

      protected override Expression VisitMethodCall( MethodCallExpression node )
      {
         if( node.Method.DeclaringType == typeof( InfluxFunctions ) )
         {
            if( node.Method.Name == "Now" )
            {
               _sb.Append( "now()" );
            }
            else if( node.Method.Name == "Count" )
            {
               _sb.Append( "COUNT(" );
               Visit( node.Arguments[ 0 ] );
               _sb.Append( ")" );
            }

            return node;
         }
         else
         {
            throw new NotSupportedException( $"The method '{node.Method.Name}' on '{node.Method.DeclaringType.FullName}' is not supported." );
         }
      }

      protected override Expression VisitMember( MemberExpression node )
      {
         if( node.Expression.NodeType == ExpressionType.Parameter )
         {
            var property = Metadata.PropertiesByClrName[ node.Member.Name ];
            _sb.Append( property.QueryProtocolEscapedKey );
            return node;
         }
         throw new NotSupportedException( $"The member '{node.Member.Name}' is not supported." );
      }

      protected override Expression VisitConstant( ConstantExpression node )
      {
         if( node.Value == null )
         {
            _sb.Append( "NULL" );
         }
         else
         {
            var value = QueryEscape.EscapeFieldOrTag( node.Value );
            _sb.Append( value );
         }
         return node;
      }
   }
}
