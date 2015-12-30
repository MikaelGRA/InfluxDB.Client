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
   public class RawTests
   {
      private InfluxClient _client;

      public RawTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Create_And_Drop_Database_Using_Execute_Operation()
      {
         var result = await _client.ExecuteOperationAsync( "CREATE DATABASE \"someDb\"" );
         Assert.True( result.Results[ 0 ].Succeeded );

         var result2 = await _client.ExecuteOperationAsync( "DROP DATABASE \"someDb\"" );
         Assert.True( result2.Results[ 0 ].Succeeded );
      }

      [Fact]
      public async Task Should_Create_And_Drop_User_Using_Execute_Operation()
      {
         var result = await _client.ExecuteOperationAsync( "CREATE USER rawUsername WITH PASSWORD '123test'" );
         Assert.True( result.Results[ 0 ].Succeeded );

         var result2 = await _client.ExecuteOperationAsync( "DROP USER rawUsername" );
         Assert.True( result2.Results[ 0 ].Succeeded );
      }
   }
}
