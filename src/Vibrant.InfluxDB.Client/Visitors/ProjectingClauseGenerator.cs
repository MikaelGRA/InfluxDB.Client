//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Metadata;

//namespace Vibrant.InfluxDB.Client.Visitors
//{
//   public class ProjectingClauseGenerator<TInfluxRow> : ExpressionVisitor
//      where TInfluxRow : new()
//   {
//      internal static readonly InfluxRowTypeInfo<TInfluxRow> Metadata = MetadataCache.GetOrCreate<TInfluxRow>();

//      public RowProjection Projection { get; protected set; }

//      public StringBuilder Clause { get; private set; }

//      public ProjectingClauseGenerator()
//      {
//         Clause = new StringBuilder();
//      }

//      protected override Expression VisitMember( MemberExpression node )
//      {
//         if( node.Expression.NodeType == ExpressionType.Parameter )
//         {
//            var targetMember = node.Member;

//            if( Projection != null )
//            {
//               var binding = Projection.Bindings.FirstOrDefault( x => x.TargetMember == node.Member );
//               OnMemberFound( binding.OriginalSourceMember );
//            }
//            else
//            {
//               OnMemberFound( node.Member );
//            }

//            return node;
//         }
//         throw new NotSupportedException( $"The member '{node.Member.Name}' is not supported." );
//      }

//      protected virtual void OnMemberFound( MemberInfo member )
//      {
//         var property = Metadata.PropertiesByClrName[ member.Name ];
//         Clause.Append( property.QueryProtocolEscapedKey );
//      }
//   }
//}
