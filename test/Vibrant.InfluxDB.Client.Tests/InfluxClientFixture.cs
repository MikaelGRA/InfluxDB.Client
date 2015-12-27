using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class InfluxClientFixture : IDisposable
   {
      public const string DatabaseName = "unittestdb";

      public InfluxClient Client { get; set; }

      public InfluxClientFixture()
      {
         Client = new InfluxClient( new Uri( "http://localhost:8086" ) );
         Client.CreateDatabaseIfNotExistsAsync( DatabaseName ).Wait();
      }

      public void Dispose()
      {
         Client.DropDatabaseIfExistsAsync( DatabaseName ).Wait();
      }
   }
}
