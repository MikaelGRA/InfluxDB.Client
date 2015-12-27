using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxResult
   {
      internal InfluxResult( string error )
      {
         ErrorMessage = error;
         Succeeded = error == null;
      }

      public string ErrorMessage { get; private set; }

      public bool Succeeded { get; private set; }
   }

   public class InfluxResult<TInfluxRow> : InfluxResult
   {
      internal InfluxResult( List<InfluxSeries<TInfluxRow>> series, string error )
         : base( error )
      {
         Series = series.AsReadOnly();
      }

      public IReadOnlyList<InfluxSeries<TInfluxRow>> Series { get; private set; }

      public InfluxSeries<TInfluxRow> FindGroup( IEnumerable<KeyValuePair<string, string>> tags )
      {
         if ( tags == null )
            throw new ArgumentNullException( nameof( tags ) );

         if ( Series.Any( x => x.GroupedTags == null ) )
            throw new InvalidOperationException( "This query result set is not grouped." );

         foreach ( var result in Series )
         {
            if ( Matches( result, tags ) )
            {
               return result;
            }
         }
         return null;
      }

      private bool Matches( InfluxSeries<TInfluxRow> result, IEnumerable<KeyValuePair<string, string>> tags )
      {
         foreach ( var tag in tags )
         {
            string tagValue;
            if ( result.GroupedTags.TryGetValue( tag.Key, out tagValue ) )
            {
               if ( tagValue != tag.Value )
               {
                  return false;
               }
            }
         }
         return true;
      }
   }
}
