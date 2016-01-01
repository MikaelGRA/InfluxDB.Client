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
      internal readonly HashSet<string> Tags;
      internal readonly HashSet<string> Fields;

      public DatabaseMeasurementInfo()
      {
         Tags = new HashSet<string>();
         Fields = new HashSet<string>();
      }
   }
}
