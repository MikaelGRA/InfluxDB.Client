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
   public class WhereClauseGenerator<TInfluxRow> : ProjectingClauseGenerator<TInfluxRow>
      where TInfluxRow : new()
   {
      private WhereClause _where;

      internal string GetWhereClause( WhereClause where )
      {
         Projection = where.Projection;
         _where = where;

         Visit( _where.Expression );
         return Clause.ToString();
      }

      protected override Expression VisitBinary( BinaryExpression b )
      {
         Clause.Append( "(" );

         // TODO: Support ~=, !~

         Visit( b.Left );

         switch( b.NodeType )
         {
            case ExpressionType.Add:
               Clause.Append( " + " );
               break;
            case ExpressionType.Subtract:
               Clause.Append( " - " );
               break;
            case ExpressionType.Multiply:
               Clause.Append( " * " );
               break;
            case ExpressionType.Divide:
               Clause.Append( " / " );
               break;
            case ExpressionType.AndAlso:
               Clause.Append( " AND " );
               break;
            case ExpressionType.OrElse:
               Clause.Append( " OR " );
               break;
            case ExpressionType.Equal:
               Clause.Append( " = " );
               break;
            case ExpressionType.NotEqual:
               Clause.Append( " <> " );
               break;
            case ExpressionType.LessThan:
               Clause.Append( " < " );
               break;
            case ExpressionType.LessThanOrEqual:
               Clause.Append( " <= " );
               break;
            case ExpressionType.GreaterThan:
               Clause.Append( " > " );
               break;
            case ExpressionType.GreaterThanOrEqual:
               Clause.Append( " >= " );
               break;
            default:
               throw new NotSupportedException( $"The binary operator '{b.NodeType}' is not supported" );
         }

         Visit( b.Right );

         Clause.Append( ")" );

         return b;
      }

      protected override Expression VisitMethodCall( MethodCallExpression node )
      {
         if( node.Method.DeclaringType == typeof( InfluxFunctions ) )
         {
            if( node.Method.Name == "Now" )
            {
               Clause.Append( "now()" );
            }
            else if( node.Method.Name == "Count" )
            {
               Clause.Append( "COUNT(" );
               Visit( node.Arguments[ 0 ] );
               Clause.Append( ")" );
            }

            return node;
         }
         else
         {
            throw new NotSupportedException( $"The method '{node.Method.Name}' on '{node.Method.DeclaringType.FullName}' is not supported." );
         }
      }

      protected override Expression VisitConstant( ConstantExpression node )
      {
         if( node.Value == null )
         {
            Clause.Append( "NULL" );
         }
         else
         {
            var value = QueryEscape.EscapeFieldOrTag( node.Value );
            Clause.Append( value );
         }
         return node;
      }
   }
}
