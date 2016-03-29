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
   internal class SelectClauseGenerator<TInfluxRow> : ProjectingClauseGenerator<TInfluxRow>
      where TInfluxRow : new()
   {
      private string _lastColumnName;
      private bool _requiresExplicitName;
      private bool _foundMember;

      internal string GetSelectClause( SelectClause clause )
      {
         Projection = clause.Projection.InnerProjection;

         var bindings = clause.Projection.Bindings; // go through a full selection expression instead of iterating bindings

         for( int i = 0 ; i < bindings.Count ; i++ )
         {
            // Should build a single expression that I can traverse here instead? Composed of all expressions... (already have that?)

            // HOW TO???
            var fullProjectionExpression = clause.Projection.GetFullProjectionExpression(); // use this instead!

            Visit( bindings[ i ].SourceExpression ); // TODO: Might miss projections, since we dont run through them recursively


            AddPostColumnString();
         }
         AddPostColumnString();

         return Clause.ToString();
      }

      private void AddPostColumnString()
      {
         if( _foundMember )
         {
            if( _requiresExplicitName )
            {
               Clause.Append( " AS " )
                  .Append( _lastColumnName );
            }

         }
         _requiresExplicitName = false;
         _foundMember = false;

         if( Clause[ Clause.Length - 1 ] == ' ' )
         {
            Clause.Remove( Clause.Length - 2, 2 );
         }

         if( _foundMember )
         {
            Clause.Append( ", " );
         }
      }

      protected override void OnMemberFound( MemberInfo member )
      {
         var property = Metadata.PropertiesByClrName[ member.Name ];
         if( property.Key != InfluxConstants.TimeColumn )
         {
            Clause.Append( property.QueryProtocolEscapedKey );
            _lastColumnName = property.QueryProtocolEscapedKey;
            _foundMember = true;
         }
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
            default:
               throw new NotSupportedException( $"The binary operator '{b.NodeType}' is not supported in a select clause." );
         }

         Visit( b.Right );

         Clause.Append( ")" );

         return b;
      }

      protected override Expression VisitMethodCall( MethodCallExpression node )
      {
         if( node.Method.DeclaringType == typeof( InfluxFunctions ) )
         {
            if( node.Method.Name == "Count" )
            {
               Clause.Append( "COUNT(" );
               Visit( node.Arguments[ 0 ] );
               Clause.Append( ")" );
            }
            else if( node.Method.Name == "Sum" )
            {
               Clause.Append( "SUM(" );
               Visit( node.Arguments[ 0 ] );
               Clause.Append( ")" );
            }

            _requiresExplicitName = true;

            return node;
         }
         else
         {
            throw new NotSupportedException( $"The method '{node.Method.Name}' on '{node.Method.DeclaringType.FullName}' is not supported in a select clause." );
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
