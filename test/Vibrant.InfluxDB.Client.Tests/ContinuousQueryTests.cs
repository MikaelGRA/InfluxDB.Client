using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;
using Xunit;

namespace Vibrant.InfluxDB.Client.Tests
{
   [Collection( "InfluxClient collection" )]
   public class ContinuousQueryTests
   {
      private InfluxClient _client;

      public ContinuousQueryTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Create_Show_And_Delete_Continuous_Query()
      {
         string continuousQuery =
@"BEGIN
  SELECT MEAN(value) INTO cpu_hourly FROM cpu GROUP BY time(1h), *
END";

         await _client.CreateContinuousQuery( InfluxClientFixture.DatabaseName, "cpu_hourly_cq", continuousQuery );

         var result = await _client.ShowContinuousQueries( InfluxClientFixture.DatabaseName );
         var cqForTestDb = result.Series.FirstOrDefault( x => x.Name == InfluxClientFixture.DatabaseName );
         Assert.NotNull( cqForTestDb );

         var series = cqForTestDb;
         Assert.Contains( series.Rows, x => x.Name == "cpu_hourly_cq" );

         await _client.DropContinuousQuery( InfluxClientFixture.DatabaseName, "cpu_hourly_cq" );

         result = await _client.ShowContinuousQueries( InfluxClientFixture.DatabaseName );
         cqForTestDb = result.Series.FirstOrDefault( x => x.Name == InfluxClientFixture.DatabaseName );
         Assert.NotNull( cqForTestDb );

         series = cqForTestDb;
         Assert.DoesNotContain( series.Rows, x => x.Name == "cpu_hourly_cq" );
      }
   }
}
