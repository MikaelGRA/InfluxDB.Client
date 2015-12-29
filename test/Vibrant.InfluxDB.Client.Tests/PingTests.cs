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
   public class PingTests
   {
      private InfluxClient _client;

      public PingTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Ping_And_Get_Version()
      {
         var result = await _client.PingAsync();

         Assert.NotNull( result.Version );
      }
   }
}
