using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// An InfluxSeries is a single series returned by InfluxDB. A single query may
   /// return multiple series due to GROUP BY.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxSeries<TInfluxRow>
   {
      internal InfluxSeries( string name, List<TInfluxRow> dataPoints, IDictionary<string, object> tags )
      {
         Name = name;
         Rows = dataPoints.AsReadOnly();
         if ( tags != null )
         {
            GroupedTags = new ReadOnlyDictionary<string, object>( tags );
         }
      }

      /// <summary>
      /// Gets the name of the measurement or series.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Gets the tags that this InfluxSeries has been grouped on.
      /// </summary>
      public IReadOnlyDictionary<string, object> GroupedTags { get; private set; }

      /// <summary>
      /// Gets the rows of the InfluxSeries.
      /// </summary>
      public IReadOnlyList<TInfluxRow> Rows { get; private set; }
   }
}
