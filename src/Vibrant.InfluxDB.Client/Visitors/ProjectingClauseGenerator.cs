using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Metadata;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class ProjectingClauseGenerator<TInfluxRow> : ExpressionVisitor
      where TInfluxRow : new()
   {
      internal static readonly InfluxRowTypeInfo<TInfluxRow> Metadata = MetadataCache.GetOrCreate<TInfluxRow>();

      private bool _isProjecting;
      private bool _isFinalProjection;
      private MemberInfo _currentProjectedTarget;

      public RowProjection InitialProjection { get; protected set; }

      public StringBuilder Clause { get; private set; }

      public ProjectingClauseGenerator()
      {
         Clause = new StringBuilder();
      }

      protected override Expression VisitMember( MemberExpression node )
      {
         if( node.Expression.NodeType == ExpressionType.Parameter )
         {
            if( _isProjecting )
            {
               _currentProjectedTarget = node.Member;

               if( _isFinalProjection )
               {
                  var property = Metadata.PropertiesByClrName[ _currentProjectedTarget.Name ];
                  Clause.Append( property.QueryProtocolEscapedKey );
               }
            }
            else
            {

               // we are already looking at the initial projections bindings, so start by looking at the inner
               var currentProjection = InitialProjection;
               _currentProjectedTarget = node.Member;

               if( currentProjection != null )
               {
                  _isProjecting = true;
                  while( currentProjection != null )
                  {
                     var binding = currentProjection.Bindings.FirstOrDefault( x => x.Target == node.Member );

                     var nextProjection = currentProjection.InnerProjection;

                     _isFinalProjection = nextProjection == null;

                     Visit( binding.Source ); // updates the _currentProjectedTarget from the parameter being used in it

                     currentProjection = nextProjection;
                  }
                  _isProjecting = false;
                  _isFinalProjection = false;
               }
               else
               {
                  var property = Metadata.PropertiesByClrName[ _currentProjectedTarget.Name ];
                  Clause.Append( property.QueryProtocolEscapedKey );
               }
            }

            return node;
         }
         throw new NotSupportedException( $"The member '{node.Member.Name}' is not supported." );
      }
   }
}
