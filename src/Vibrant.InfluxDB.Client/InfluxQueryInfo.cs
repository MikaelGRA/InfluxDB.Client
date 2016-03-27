using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Visitors;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxQueryInfo<TInfluxRow>
      where TInfluxRow : new()
   {
      private static readonly InfluxRowTypeInfo<TInfluxRow> Metadata = MetadataCache.GetOrCreate<TInfluxRow>();

      public string Database { get; private set; }

      public string MeasurementName { get; private set; }

      public InfluxQueryInfo( string db, string measurementName )
      {
         Database = db;
         MeasurementName = measurementName;
         WhereClauses = new List<WhereClause>();
      }

      public List<WhereClause> WhereClauses { get; private set; }

      public SelectClause SelectClause { get; set; }

      public string GenerateInfluxQL()
      {
         var sb = new StringBuilder();
         // create selection
         sb.Append( "SELECT " );

         // items has not been projected
         if( SelectClause == null )
         {
            var fields = Metadata.Fields.Union( Metadata.Tags ).ToList();
            for( int i = 0 ; i < fields.Count ; i++ )
            {
               var field = fields[ i ];
               sb.Append( field.QueryProtocolEscapedKey );
               if( i != fields.Count - 1 )
               {
                  sb.Append( ", " );
               }
            }
         }
         else // items has been projected
         {
            sb.Append( new SelectClauseGenerator<TInfluxRow>().GetSelectClause( SelectClause ) );
         }


         sb.Append( " FROM " );
         sb.Append( QueryEscape.EscapeKey( MeasurementName ) );
         sb.AppendLine();

         if( WhereClauses.Count > 0 )
         {
            sb.Append( "WHERE " );
            for( int i = 0 ; i < WhereClauses.Count ; i++ )
            {
               sb.Append( new WhereClauseGenerator<TInfluxRow>().GetWhereClause( WhereClauses[ i ] ) );

               // if not last
               if( i != WhereClauses.Count - 1 )
               {
                  sb.Append( " AND " );
               }
            }
         }
         return sb.ToString();
      }
   }
}
