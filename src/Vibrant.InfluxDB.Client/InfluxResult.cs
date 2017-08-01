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
      public InfluxResult( int statementId, string error )
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
      public int StatementId { get; private set; }

      public void AppendErrorMessage( string message )
      {
         if( message != null )
         {
            if( ErrorMessage != null )
            {
               ErrorMessage += Environment.NewLine;
            }
            ErrorMessage += message;

            Succeeded = false;
         }
      }
   }

   /// <summary>
   /// Result of a query to influxdb that returns one or more tables.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxResult<TInfluxRow> : InfluxResult
   {
      public InfluxResult( int statementId, string error )
         : this( statementId, error, new List<InfluxSeries<TInfluxRow>>() )
      {
         Series = new List<InfluxSeries<TInfluxRow>>();
      }

      public InfluxResult( int statementId, string error, List<InfluxSeries<TInfluxRow>> rows )
         : base( statementId, error )
      {
         Series = rows;
      }

      /// <summary>
      /// Gets the series.
      /// </summary>
      public List<InfluxSeries<TInfluxRow>> Series { get; private set; }

      /// <summary>
      /// Finds the serie that can be identified by the specified tags. 
      /// This method is obsolete and will be removed in the next major upcoming release. 
      /// Use the overload that requires a name instead.
      /// </summary>
      /// <param name="tags"></param>
      /// <returns></returns>
      [Obsolete( "Method will be removed in next major release. Use overload that requires a name." )]
      public InfluxSeries<TInfluxRow> FindGroup( IEnumerable<KeyValuePair<string, object>> tags )
      {
         if( tags == null )
            throw new ArgumentNullException( nameof( tags ) );

         if( Series.Any( x => x.GroupedTags == null ) )
            throw new InvalidOperationException( "This query result set is not grouped." );

         return FindGroupInternal( null, tags, false );
      }

      /// <summary>
      /// Finds the serie that can be identified by the specified tags and name.
      /// </summary>
      /// <param name="seriesName"></param>
      /// <param name="tags"></param>
      /// <returns></returns>
      public InfluxSeries<TInfluxRow> FindGroup( string seriesName, IEnumerable<KeyValuePair<string, object>> tags )
      {
         if( tags == null )
            throw new ArgumentNullException( nameof( tags ) );

         if( Series.Any( x => x.GroupedTags == null ) )
            throw new InvalidOperationException( "This query result set is not grouped." );

         return FindGroupInternal( seriesName, tags, true );
      }

      public void AddInfluxSeries( InfluxSeries<TInfluxRow> series )
      {
         Series.Add( series );
      }

      internal InfluxSeries<TInfluxRow> FindGroupInternal( string seriesName, IEnumerable<KeyValuePair<string, object>> tags, bool requireNameComparison )
      {
         foreach( var result in Series )
         {
            if( Matches( result, seriesName, tags, requireNameComparison ) )
            {
               return result;
            }
         }
         return null;
      }


      private bool Matches( InfluxSeries<TInfluxRow> result, string seriesName, IEnumerable<KeyValuePair<string, object>> tags, bool requireNameComparison )
      {
         if( requireNameComparison && result.Name != seriesName )
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
