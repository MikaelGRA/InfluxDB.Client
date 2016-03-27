using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Linq;

namespace Vibrant.InfluxDB.Client.Visitors
{
   internal class InfluxQueryInfoGenerator<TInfluxRow> : ExpressionVisitor
      where TInfluxRow : new()
   {
      private InfluxQueryInfo<TInfluxRow> _info;
      private RowProjection _currentProjection;

      internal InfluxQueryInfoGenerator()
      {

      }

      internal InfluxQueryInfo<TInfluxRow> GetInfo( Expression expression, string db, string measurementName )
      {
         _info = new InfluxQueryInfo<TInfluxRow>( db, measurementName );
         Visit( expression );
         return _info;
      }

      private static Expression StripQuotes( Expression e )
      {
         while( e.NodeType == ExpressionType.Quote )
         {
            e = ( (UnaryExpression)e ).Operand;
         }
         return e;
      }

      protected override Expression VisitMethodCall( MethodCallExpression node )
      {
         if( node.Method.DeclaringType == typeof( Queryable ) )
         {
            // Visit the SOURCE itself (the object the method was called on)
            // source.MethodName( expression )
            //  -> source is node.Arguments[ 0 ]
            //  -> expression is node.Arguments[ 1 ]

            if( node.Method.Name == "Where" )
            {
               Visit( node.Arguments[ 0 ] );

               var lambda = (LambdaExpression)StripQuotes( node.Arguments[ 1 ] );

               // store the Body of the lambda (representing part of the Where clause)
               _info.IncludeWhere( lambda.Body, _currentProjection );

               // we do not visit the body itself, we will visit that later to perform query creation
            }
            else if( node.Method.Name == "Select" )
            {
               Visit( node.Arguments[ 0 ] );

               var lambda = (LambdaExpression)StripQuotes( node.Arguments[ 1 ] );

               // store information about the projection and which columns were selected

               // a chain of the lambdas that were selected represents the projection itself
               _currentProjection = new RowProjection();
               Visit( lambda.Body );
            }
            else
            {
               throw new NotSupportedException( $"The method '{node.Method.Name}' is not supported." );
            }
         }

         return node;
      }

      protected override MemberAssignment VisitMemberAssignment( MemberAssignment node )
      {
         var cb = new ColumnBinding
         {
            Source = node.Expression,
            Target = node.Member,
         };
         _currentProjection.Bindings.Add( cb );

         return node;
      }

      //protected override Expression VisitNew( NewExpression node )
      //{
      //}
   }
}
