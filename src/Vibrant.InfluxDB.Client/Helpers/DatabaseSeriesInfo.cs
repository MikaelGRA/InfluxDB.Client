using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal struct DatabaseSeriesInfoKey
   {
      public DatabaseSeriesInfoKey( string db, string seriesName )
      {
         Database = db;
         SeriesName = seriesName;
      }

      internal readonly string Database;

      internal readonly string SeriesName;
   }

   internal class DatabaseSeriesInfo
   {
      internal readonly HashSet<string> Tags;

      internal readonly HashSet<string> Fields;

      public DatabaseSeriesInfo()
      {
         Tags = new HashSet<string>();
         Fields = new HashSet<string>();
      }
   }
}
