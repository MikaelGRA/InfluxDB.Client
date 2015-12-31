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
      internal InfluxResult( string error )
      {
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
   }

   /// <summary>
   /// Result of a query to influxdb that returns one or more tables.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxResult<TInfluxRow> : InfluxResult
   {
      internal InfluxResult( List<InfluxSeries<TInfluxRow>> series, string error )
         : base( error )
      {
         Series = series.AsReadOnly();
      }

      /// <summary>
      /// Gets the series.
      /// </summary>
      public IReadOnlyList<InfluxSeries<TInfluxRow>> Series { get; private set; }

      /// <summary>
      /// Finds the serie that can be identified by the specified tags.
      /// </summary>
      /// <param name="tags"></param>
      /// <returns></returns>
      public InfluxSeries<TInfluxRow> FindGroup( IEnumerable<KeyValuePair<string, object>> tags )
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

      private bool Matches( InfluxSeries<TInfluxRow> result, IEnumerable<KeyValuePair<string, object>> tags )
      {
         foreach ( var tag in tags )
         {
            object tagValue;
            if ( result.GroupedTags.TryGetValue( tag.Key, out tagValue ) )
            {
               if ( tagValue != null )
               {
                  if ( !tagValue.Equals( tag.Value ) )
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
