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
   public class DatabaseManagementTests
   {
      private const string Unused = "unuseddatabasename";

      private InfluxClient _client;

      public DatabaseManagementTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      private ComputerInfo[] CreateTypedRowsStartingAt( DateTime start, int rows, bool includeNulls )
      {
         var rng = new Random();
         var regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
         var hosts = new[] { "ma-lt", "surface-book" };

         var timestamp = start;
         var infos = new ComputerInfo[ rows ];
         for ( int i = 0 ; i < rows ; i++ )
         {
            long ram = rng.Next( int.MaxValue );
            double cpu = rng.NextDouble();
            string region = regions[ rng.Next( regions.Length ) ];
            string host = hosts[ rng.Next( hosts.Length ) ];

            if ( includeNulls )
            {
               var info = new ComputerInfo { Timestamp = timestamp, RAM = ram, Host = host, Region = region };
               infos[ i ] = info;
            }
            else
            {
               var info = new ComputerInfo { Timestamp = timestamp, CPU = cpu, RAM = ram, Host = host, Region = region };
               infos[ i ] = info;
            }

            timestamp = timestamp.AddSeconds( 1 );
         }

         return infos;
      }

      private DynamicInfluxRow[] CreateDynamicRowsStartingAt( DateTime start, int rows )
      {
         var rng = new Random();
         var regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
         var hosts = new[] { "ma-lt", "surface-book" };
         
         var timestamp = start;
         var infos = new DynamicInfluxRow[ rows ];
         for ( int i = 0 ; i < rows ; i++ )
         {
            long ram = rng.Next( int.MaxValue );
            double cpu = rng.NextDouble();
            string region = regions[ rng.Next( regions.Length ) ];
            string host = hosts[ rng.Next( hosts.Length ) ];

            var info = new DynamicInfluxRow();
            info.Fields.Add( "cpu", cpu );
            info.Fields.Add( "ram", ram );
            info.Tags.Add( "host", host );
            info.Tags.Add( "region", region );
            info.Timestamp = timestamp;

            infos[ i ] = info;

            timestamp = timestamp.AddSeconds( 1 );
         }
         return infos;
      }

      [Fact]
      public async Task Should_Show_Database()
      {
         var result = await _client.ShowDatabasesAsync();

         Assert.True( result.Succeeded );
         Assert.Equal( result.Series.Count, 1 );

         var rows = result.Series[ 0 ].Rows;
         Assert.Contains( rows, x => x.Name == InfluxClientFixture.DatabaseName );
      }

      [Fact]
      public async Task Should_Create_Show_And_Delete_Database()
      {
         await _client.CreateDatabaseIfNotExistsAsync( Unused );

         var result = await _client.ShowDatabasesAsync();

         Assert.True( result.Succeeded );
         Assert.Equal( result.Series.Count, 1 );

         var rows = result.Series[ 0 ].Rows;
         Assert.Contains( rows, x => x.Name == Unused );

         await _client.DropDatabaseIfExistsAsync( Unused );
      }

      [Fact]
      public async Task Should_Throw_When_Creating_Duplicate_Database()
      {
         await _client.CreateDatabaseIfNotExistsAsync( Unused );

         await Assert.ThrowsAsync( typeof( InfluxException ), async () =>
         {
            await _client.CreateDatabaseAsync( Unused );
         } );

         await _client.DropDatabaseAsync( Unused );
      }

      [Fact]
      public async Task Should_Create_And_Drop_Series()
      {
         var state = new ComputerInfo
         {
            CPU = 0.42,
            RAM = 1024,
            Host = "some-host",
            Region = "some-region",
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes( 5 ),
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "someSeries", new[] { state }, TimestampPrecision.Nanosecond, Consistency.One );

         var resultSet1 = await _client.ReadAsync<ComputerInfo>( "SELECT * FROM someSeries WHERE region = 'some-region' AND host = 'some-host'", InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, resultSet1.Results.Count );

         var result1 = resultSet1.Results[ 0 ];
         Assert.Equal( 1, result1.Series.Count );

         var series1 = result1.Series[ 0 ];
         Assert.Equal( 1, series1.Rows.Count );

         await _client.DropSeries( InfluxClientFixture.DatabaseName, "someSeries", "region = 'some-region' AND host = 'some-host'" );

         var resultSet2 = await _client.ReadAsync<ComputerInfo>( "SELECT * FROM someSeries WHERE region = 'some-region' AND host = 'some-host'", InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, resultSet2.Results.Count );

         var result2 = resultSet2.Results[ 0 ];
         Assert.Equal( 0, result2.Series.Count );
         Assert.False( result2.Succeeded );
      }
   }
}
