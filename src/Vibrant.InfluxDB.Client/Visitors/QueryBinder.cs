//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Helpers;
//using Vibrant.InfluxDB.Client.Linq;

//namespace Vibrant.InfluxDB.Client.Visitors
//{
//   internal class QueryBinder<TInfluxRow> : ExpressionVisitor
//   {
//      private InfluxLinqQuery<TInfluxRow> _linq;
//      private InfluxRowTypeInfo<TInfluxRow> _rowType;
//      private bool _selectTags;

//      internal QueryBinder()
//      {
//      }

//      private bool CanBeColumn( Expression expression )
//      {
//         return expression.NodeType == (ExpressionType)InfluxExpressionType.Column;
//      }

//      internal Expression Bind( Expression expression )
//      {
//         this.map = new Dictionary<ParameterExpression, Expression>();
//         return this.Visit( expression );
//      }

//      private static Expression StripQuotes( Expression e )
//      {
//         while ( e.NodeType == ExpressionType.Quote )
//         {
//            e = ( (UnaryExpression)e ).Operand;
//         }
//         return e;
//      }

//      private string GetNextAlias()
//      {
//         return "t" + ( aliasCount++ );
//      }

//      private ProjectedColumns ProjectColumns( Expression expression, string newAlias, string existingAlias )
//      {
//         return this.columnProjector.ProjectColumns( expression, newAlias, existingAlias );
//      }

//      protected override Expression VisitMethodCall( MethodCallExpression m )
//      {
//         if ( m.Method.DeclaringType == typeof( Queryable ) ||
//             m.Method.DeclaringType == typeof( Enumerable ) )
//         {
//            switch ( m.Method.Name )
//            {
//               case "Where":
//                  return BindWhere( m.Type, m.Arguments[ 0 ], (LambdaExpression)StripQuotes( m.Arguments[ 1 ] ) );
//            }
//            throw new NotSupportedException( string.Format( "The method '{0}' is not supported", m.Method.Name ) );
//         }
//         return base.VisitMethodCall( m );
//      }

//      private Expression BindWhere( Type resultType, Expression source, LambdaExpression predicate )
//      {
//         ProjectionExpression projection = (ProjectionExpression)this.Visit( source );
//         this.map[ predicate.Parameters[ 0 ] ] = projection.Projector;
//         Expression where = this.Visit( predicate.Body );
//         string alias = this.GetNextAlias();
//         ProjectedColumns pc = this.ProjectColumns( projection.Projector, alias, GetExistingAlias( projection.Source ) );
//         return new ProjectionExpression(
//             new SelectExpression( resultType, alias, pc.Columns, projection.Source, where ),
//             pc.Projector
//             );
//      }

//      private static string GetExistingAlias( Expression source )
//      {
//         switch ( (InfluxExpressionType)source.NodeType )
//         {
//            case InfluxExpressionType.Select:
//               return ( (SelectExpression)source ).Alias;
//            case InfluxExpressionType.Table:
//               return ( (TableExpression)source ).Alias;
//            default:
//               throw new InvalidOperationException( string.Format( "Invalid source node type '{0}'", source.NodeType ) );
//         }
//      }

//      private bool IsTable( object value )
//      {
//         IQueryable q = value as IQueryable;
//         return q != null && q.Expression.NodeType == ExpressionType.Constant;
//      }

//      private string GetTableName( object table )
//      {
//         IQueryable tableQuery = (IQueryable)table;
//         Type rowType = tableQuery.ElementType;
//         return rowType.Name;
//      }

//      private string GetColumnName( MemberInfo member )
//      {
//         return member.Name;
//      }

//      private Type GetColumnType( MemberInfo member )
//      {
//         FieldInfo fi = member as FieldInfo;
//         if ( fi != null )
//         {
//            return fi.FieldType;
//         }
//         PropertyInfo pi = (PropertyInfo)member;
//         return pi.PropertyType;
//      }

//      private IEnumerable<MemberInfo> GetMappedMembers( Type rowType )
//      {
//         return rowType.GetProperties();
//      }

//      protected override Expression VisitConstant( ConstantExpression c )
//      {
//         if ( IsTable( c.Value ) )
//         {
//            IQueryable table = (IQueryable)c.Value;
//            foreach ( var field in _rowType.Fields )
//            {
//               // construct select clause...
//               _linq.Selects.Add( new FieldSelect<TInfluxRow>( field.Value ) );
//            }

//            if ( _selectTags )
//            {
//               foreach ( var tag in _rowType.Tags )
//               {
//                  _linq.Selects.Add( new FieldSelect<TInfluxRow>( tag.Value ) );
//               }
//            }
//         }
//         return c;
//      }

//      protected override Expression VisitParameter( ParameterExpression p )
//      {
//         Expression e;
//         if ( this.map.TryGetValue( p, out e ) )
//         {
//            return e;
//         }
//         return p;
//      }

//      protected override Expression VisitMember( MemberExpression m )
//      {
//         Expression source = this.Visit( m.Expression );
//         switch ( source.NodeType )
//         {
//            case ExpressionType.MemberInit:
//               MemberInitExpression min = (MemberInitExpression)source;
//               for ( int i = 0, n = min.Bindings.Count ; i < n ; i++ )
//               {
//                  MemberAssignment assign = min.Bindings[ i ] as MemberAssignment;
//                  if ( assign != null && MembersMatch( assign.Member, m.Member ) )
//                  {
//                     return assign.Expression;
//                  }
//               }
//               break;
//            case ExpressionType.New:
//               NewExpression nex = (NewExpression)source;
//               if ( nex.Members != null )
//               {
//                  for ( int i = 0, n = nex.Members.Count ; i < n ; i++ )
//                  {
//                     if ( MembersMatch( nex.Members[ i ], m.Member ) )
//                     {
//                        return nex.Arguments[ i ];
//                     }
//                  }
//               }
//               break;
//         }
//         if ( source == m.Expression )
//         {
//            return m;
//         }
//         return MakeMemberAccess( source, m.Member );
//      }

//      private bool MembersMatch( MemberInfo a, MemberInfo b )
//      {
//         if ( a == b )
//         {
//            return true;
//         }
//         if ( a is MethodInfo && b is PropertyInfo )
//         {
//            return a == ( (PropertyInfo)b ).GetGetMethod();
//         }
//         else if ( a is PropertyInfo && b is MethodInfo )
//         {
//            return ( (PropertyInfo)a ).GetGetMethod() == b;
//         }
//         return false;
//      }

//      private Expression MakeMemberAccess( Expression source, MemberInfo mi )
//      {
//         FieldInfo fi = mi as FieldInfo;
//         if ( fi != null )
//         {
//            return Expression.Field( source, fi );
//         }
//         PropertyInfo pi = (PropertyInfo)mi;
//         return Expression.Property( source, pi );
//      }
//   }
//}
