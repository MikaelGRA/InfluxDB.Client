//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Metadata;
//using Vibrant.InfluxDB.Client.Parsers;
//using Vibrant.InfluxDB.Client.Visitors;

//namespace Vibrant.InfluxDB.Client
//{
//   public class InfluxQueryInfo<TInfluxRow>
//      where TInfluxRow : new()
//   {
//      private static readonly InfluxRowTypeInfo<TInfluxRow> Metadata = MetadataCache.GetOrCreate<TInfluxRow>();

//      public string Database { get; private set; }

//      public string MeasurementName { get; private set; }

//      public InfluxQueryInfo( string db, string measurementName )
//      {
//         Database = db;
//         MeasurementName = measurementName;
//         WhereClauses = new List<WhereClause>();
//         OrderByClauses = new List<OrderByClause>();
//      }

//      public List<WhereClause> WhereClauses { get; private set; }

//      public List<OrderByClause> OrderByClauses { get; private set; }

//      public SelectClause SelectClause { get; set; }

//      public int? Take { get; set; }

//      public int? Skip { get; set; }

//      public TimeSpan? GroupByTime { get; set; }

//      public string GenerateInfluxQL()
//      {
//         var sb = new StringBuilder();
//         // create selection
//         sb.Append( "SELECT " );

//         // items has not been projected
//         if( SelectClause == null )
//         {
//            var fields = Metadata.Fields.Union( Metadata.Tags ).ToList();
//            for( int i = 0 ; i < fields.Count ; i++ )
//            {
//               var field = fields[ i ];
//               sb.Append( field.QueryProtocolEscapedKey );
//               if( i != fields.Count - 1 )
//               {
//                  sb.Append( ", " );
//               }
//            }
//         }
//         else // items has been projected
//         {
//            sb.Append( new SelectClauseGenerator<TInfluxRow>().GetSelectClause( SelectClause ) );
//         }


//         sb.Append( " FROM " );
//         sb.Append( QueryEscape.EscapeKey( MeasurementName ) );

//         if( WhereClauses.Count > 0 )
//         {
//            sb.AppendLine();
//            sb.Append( "WHERE " );

//            for( int i = 0 ; i < WhereClauses.Count ; i++ )
//            {
//               sb.Append( new WhereClauseGenerator<TInfluxRow>().GetWhereClause( WhereClauses[ i ] ) );

//               // if not last
//               if( i != WhereClauses.Count - 1 )
//               {
//                  sb.Append( " AND " );
//               }
//            }
//         }

//         if( GroupByTime.HasValue )
//         {
//            sb.AppendLine();
//            sb.Append( "GROUP BY time(" )
//               .Append( GroupByTime.Value.ToInfluxTimeSpan( true ) )
//               .Append( ")" );
//         }

//         if( OrderByClauses.Count > 0 )
//         {
//            sb.AppendLine();
//            sb.Append( "ORDER BY " );

//            for( int i = 0 ; i < OrderByClauses.Count ; i++ )
//            {
//               sb.Append( new OrderByClauseGenerator<TInfluxRow>().GetOrderByClause( OrderByClauses[ i ] ) );

//               // if not last
//               if( i != OrderByClauses.Count - 1 )
//               {
//                  sb.Append( ", " );
//               }
//            }
//         }

//         if( Take.HasValue || Skip.HasValue )
//         {
//            sb.AppendLine();
//            if( Take.HasValue )
//            {
//               sb.Append( "LIMIT " ).Append( Take.Value ).Append( ' ' );
//            }

//            if( Skip.HasValue )
//            {
//               sb.Append( "OFFSET " ).Append( Skip.Value );
//            }
//         }

//         return sb.ToString();
//      }
//   }
//}
