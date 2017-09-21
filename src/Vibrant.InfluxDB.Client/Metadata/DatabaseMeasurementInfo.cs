using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Metadata
{
   internal struct DatabaseMeasurementInfoKey
   {
      internal readonly string Database;
      internal readonly string MeasurementName;

      public DatabaseMeasurementInfoKey( string db, string measurementName )
      {
         Database = db;
         MeasurementName = measurementName;
      }
   }

   internal class DatabaseMeasurementInfo
   {
      internal readonly DateTime Timestamp;
      internal readonly HashSet<string> Tags;

      public DatabaseMeasurementInfo( DateTime timestamp )
      {
         Timestamp = timestamp;
         Tags = new HashSet<string>();
      }
   }
}
