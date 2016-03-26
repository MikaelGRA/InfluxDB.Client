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
      }

      public Expression Where { get; private set; }

      public void IncludeWhere( Expression where )
      {
         if( Where == null )
         {
            Where = where;
         }
         else
         {
            Where = Expression.AndAlso( Where, where );
         }
      }

      public string GenerateInfluxQL()
      {
         var sb = new StringBuilder();
         // create selection
         sb.Append( "SELECT " );
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
         sb.Append( " FROM " );
         sb.Append( QueryEscape.EscapeKey( MeasurementName ) );
         sb.AppendLine();

         if( Where != null )
         {
            sb.Append( new WhereClauseGenerator<TInfluxRow>().GetWhereClause( Where ) );
         }
         return sb.ToString();
      }
   }
}
