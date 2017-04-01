using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Result of a query to influxdb that does not return a table.
   /// </summary>
   public class InfluxResult
   {
      internal InfluxResult( int statementId, string error )
      {
         StatementId = statementId;
         ErrorMessage = error;
         Succeeded = error == null;
      }

      /// <summary>
      /// Gets the error message, if the operation did not succeed.
      /// </summary>
      public string ErrorMessage { get; private set; }

      /// <summary>
      /// Gets an indication of whether the operation succeeded.
      /// </summary>
      public bool Succeeded { get; private set; }

      /// <summary>
      /// Gets or sets the statement id.
      /// </summary>
      public int StatementId { get; set; }
   }

   /// <summary>
   /// Result of a query to influxdb that returns one or more tables.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxResult<TInfluxRow> : InfluxResult
   {
      private List<InfluxSeries<TInfluxRow>> _series;

      internal InfluxResult( int statementId, string error )
         : base( statementId, error )
      {
         _series = new List<InfluxSeries<TInfluxRow>>();
      }

      /// <summary>
      /// Gets the series.
      /// </summary>
      public IReadOnlyList<InfluxSeries<TInfluxRow>> Series => _series;

      /// <summary>
      /// Finds the serie that can be identified by the specified tags.
      /// </summary>
      /// <param name="tags"></param>
      /// <returns></returns>
      public InfluxSeries<TInfluxRow> FindGroup( string seriesName, IEnumerable<KeyValuePair<string, object>> tags )
      {
         if( tags == null )
            throw new ArgumentNullException( nameof( tags ) );

         if( Series.Any( x => x.GroupedTags == null ) )
            throw new InvalidOperationException( "This query result set is not grouped." );

         return FindGroupInternal( seriesName, tags );
      }

      internal void AddInfluxSeries( InfluxSeries<TInfluxRow> series )
      {
         _series.Add( series );
      }

      internal InfluxSeries<TInfluxRow> FindGroupInternal( string seriesName, IEnumerable<KeyValuePair<string, object>> tags )
      {
         foreach( var result in Series )
         {
            if( Matches( result, seriesName, tags ) )
            {
               return result;
            }
         }
         return null;
      }


      private bool Matches( InfluxSeries<TInfluxRow> result, string seriesName, IEnumerable<KeyValuePair<string, object>> tags )
      {
         if( result.Name != seriesName )
         {
            return false;
         }

         if( result.GroupedTags == null && ( tags == null || tags.Count() == 0 ) )
         {
            return true;
         }

         foreach( var tag in tags )
         {
            object tagValue;
            if( result.GroupedTags.TryGetValue( tag.Key, out tagValue ) )
            {
               if( tagValue != null )
               {
                  if( !tagValue.Equals( tag.Value ) )
                  {
                     return false;
                  }
               }
               else
               {
                  return tag.Value == null;
               }
            }
         }
         return true;
      }
   }
}
