using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
    public class InfluxResultSet<TInfluxRow>
    {
      internal InfluxResultSet( List<InfluxResult<TInfluxRow>> results )
      {
         Results = results.AsReadOnly();
      }

      public IReadOnlyList<InfluxResult<TInfluxRow>> Results { get; private set; }
   }

   public class InfluxResultSet
   {
      internal InfluxResultSet( List<InfluxResult> results )
      {
         Results = results.AsReadOnly();
      }

      public IReadOnlyList<InfluxResult> Results { get; private set; }
   }
}
