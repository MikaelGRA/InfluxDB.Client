using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxSeries<TInfluxRow>
   {
      internal InfluxSeries( string name, List<TInfluxRow> dataPoints, IDictionary<string, string> tags )
      {
         Name = name;
         Rows = dataPoints.AsReadOnly();
         if ( tags != null )
         {
            GroupedTags = new ReadOnlyDictionary<string, string>( tags );
         }
      }

      public string Name { get; private set; }

      public IReadOnlyDictionary<string, string> GroupedTags { get; private set; }

      public IReadOnlyList<TInfluxRow> Rows { get; private set; }
   }
}
