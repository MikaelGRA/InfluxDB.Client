using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Result of multiple queries against influxDB that does not return a table.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   public class InfluxResultSet<TInfluxRow>
   {
      internal InfluxResultSet( List<InfluxResult<TInfluxRow>> results )
      {
         Results = results.AsReadOnly();
      }

      /// <summary>
      /// Gets the results.
      /// </summary>
      public IReadOnlyList<InfluxResult<TInfluxRow>> Results { get; private set; }
   }

   /// <summary>
   /// Result of multiple queries against influxDB that returns a table.
   /// </summary>
   public class InfluxResultSet
   {
      internal InfluxResultSet( List<InfluxResult> results )
      {
         Results = results.AsReadOnly();
      }

      /// <summary>
      /// Gets the results.
      /// </summary>
      public IReadOnlyList<InfluxResult> Results { get; private set; }
   }
}
