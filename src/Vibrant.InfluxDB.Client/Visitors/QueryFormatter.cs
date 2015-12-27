//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Vibrant.InfluxDB.Client.Visitors
//{
//   internal class QueryFormatter : InfluxExpressionVisitor
//   {
//      private StringBuilder sb;
//      private int indent = 2;
//      private int depth;

//      internal QueryFormatter()
//      {
//      }

//      internal string Format( Expression expression )
//      {
//         this.sb = new StringBuilder();
//         this.Visit( expression );
//         return this.sb.ToString();
//      }

//      protected enum Identation
//      {
//         Same,
//         Inner,
//         Outer
//      }

//      internal int IdentationWidth
//      {
//         get { return this.indent; }
//         set { this.indent = value; }
//      }

//      private void AppendNewLine( Identation style )
//      {
//         sb.AppendLine();
//         if ( style == Identation.Inner )
//         {
//            this.depth++;
//         }
//         else if ( style == Identation.Outer )
//         {
//            this.depth--;
//            System.Diagnostics.Debug.Assert( this.depth >= 0 );
//         }
//         for ( int i = 0, n = this.depth * this.indent ; i < n ; i++ )
//         {
//            sb.Append( " " );
//         }
//      }

//      protected override Expression VisitMethodCall( MethodCallExpression m )
//      {
//         throw new NotSupportedException( string.Format( "The method '{0}' is not supported", m.Method.Name ) );
//      }

//      protected override Expression VisitUnary( UnaryExpression u )
//      {
//         switch ( u.NodeType )
//         {
//            case ExpressionType.Not:
//               sb.Append( " NOT " );
//               this.Visit( u.Operand );
//               break;
//            default:
//               throw new NotSupportedException( string.Format( "The unary operator '{0}' is not supported", u.NodeType ) );
//         }
//         return u;
//      }

//      protected override Expression VisitBinary( BinaryExpression b )
//      {
//         sb.Append( "(" );
//         this.Visit( b.Left );
//         switch ( b.NodeType )
//         {
//            case ExpressionType.AndAlso:
//               sb.Append( " AND " );
//               break;
//            case ExpressionType.OrElse:
//               sb.Append( " OR" );
//               break;
//            case ExpressionType.Equal:
//               sb.Append( " = " );
//               break;
//            case ExpressionType.NotEqual:
//               sb.Append( " <> " );
//               break;
//            case ExpressionType.LessThan:
//               sb.Append( " < " );
//               break;
//            case ExpressionType.LessThanOrEqual:
//               sb.Append( " <= " );
//               break;
//            case ExpressionType.GreaterThan:
//               sb.Append( " > " );
//               break;
//            case ExpressionType.GreaterThanOrEqual:
//               sb.Append( " >= " );
//               break;
//            default:
//               throw new NotSupportedException( string.Format( "The binary operator '{0}' is not supported", b.NodeType ) );
//         }
//         this.Visit( b.Right );
//         sb.Append( ")" );
//         return b;
//      }

//      protected override Expression VisitConstant( ConstantExpression c )
//      {
//         if ( c.Value == null )
//         {
//            sb.Append( "NULL" );
//         }
//         else
//         {
//            switch ( Type.GetTypeCode( c.Value.GetType() ) )
//            {
//               case TypeCode.Boolean:
//                  sb.Append( ( (bool)c.Value ) ? 1 : 0 );
//                  break;
//               case TypeCode.String:
//                  sb.Append( "'" );
//                  sb.Append( c.Value );
//                  sb.Append( "'" );
//                  break;
//               case TypeCode.Object:
//                  throw new NotSupportedException( string.Format( "The constant for '{0}' is not supported", c.Value ) );
//               default:
//                  sb.Append( c.Value );
//                  break;
//            }
//         }
//         return c;
//      }

//      protected override Expression VisitColumn( ColumnExpression column )
//      {
//         if ( !string.IsNullOrEmpty( column.Alias ) )
//         {
//            sb.Append( column.Alias );
//            sb.Append( "." );
//         }
//         sb.Append( column.Name );
//         return column;
//      }

//      protected override Expression VisitSelect( SelectExpression select )
//      {
//         sb.Append( "SELECT " );
//         for ( int i = 0, n = select.Columns.Count ; i < n ; i++ )
//         {
//            ColumnDeclaration column = select.Columns[ i ];
//            if ( i > 0 )
//            {
//               sb.Append( ", " );
//            }
//            ColumnExpression c = this.Visit( column.Expression ) as ColumnExpression;
//            if ( c == null || c.Name != select.Columns[ i ].Name )
//            {
//               sb.Append( " AS " );
//               sb.Append( column.Name );
//            }
//         }
//         if ( select.From != null )
//         {
//            this.AppendNewLine( Identation.Same );
//            sb.Append( "FROM " );
//            this.VisitSource( select.From );
//         }
//         if ( select.Where != null )
//         {
//            this.AppendNewLine( Identation.Same );
//            sb.Append( "WHERE " );
//            this.Visit( select.Where );
//         }
//         return select;
//      }

//      protected override Expression VisitSource( Expression source )
//      {
//         switch ( (InfluxExpressionType)source.NodeType )
//         {
//            case InfluxExpressionType.Table:
//               TableExpression table = (TableExpression)source;
//               sb.Append( table.Name );
//               sb.Append( " AS " );
//               sb.Append( table.Alias );
//               break;
//            case InfluxExpressionType.Select:
//               SelectExpression select = (SelectExpression)source;
//               sb.Append( "(" );
//               this.AppendNewLine( Identation.Inner );
//               this.Visit( select );
//               this.AppendNewLine( Identation.Outer );
//               sb.Append( ")" );
//               sb.Append( " AS " );
//               sb.Append( select.Alias );
//               break;
//            default:
//               throw new InvalidOperationException( "Select source is not valid type" );
//         }
//         return source;
//      }
//   }
//}
