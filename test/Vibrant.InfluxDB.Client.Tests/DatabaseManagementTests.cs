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

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "dmt1", new[] { state } );

         var resultSet1 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt1 WHERE region = 'some-region' AND host = 'some-host'" );
         Assert.Equal( 1, resultSet1.Results.Count );

         var result1 = resultSet1.Results[ 0 ];
         Assert.Equal( 1, result1.Series.Count );

         var series1 = result1.Series[ 0 ];
         Assert.Equal( 1, series1.Rows.Count );

         await _client.DropSeries( InfluxClientFixture.DatabaseName, "dmt1", "region = 'some-region' AND host = 'some-host'" );

         var resultSet2 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt1 WHERE region = 'some-region' AND host = 'some-host'" );
         Assert.Equal( 1, resultSet2.Results.Count );

         var result2 = resultSet2.Results[ 0 ];
         Assert.Equal( 0, result2.Series.Count );
         Assert.False( result2.Succeeded );
      }

      [Fact]
      public async Task Should_Create_And_Drop_Measurement()
      {
         var state = new ComputerInfo
         {
            CPU = 0.42,
            RAM = 1024,
            Host = "some-host",
            Region = "some-region",
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes( 5 ),
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "dmt2", new[] { state } );

         var resultSet1 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt2 WHERE region = 'some-region' AND host = 'some-host'" );
         Assert.Equal( 1, resultSet1.Results.Count );

         var result1 = resultSet1.Results[ 0 ];
         Assert.Equal( 1, result1.Series.Count );

         var series1 = result1.Series[ 0 ];
         Assert.Equal( 1, series1.Rows.Count );

         await _client.DropMeasurementAsync( InfluxClientFixture.DatabaseName, "dmt2" );

         var resultSet2 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt2 WHERE region = 'some-region' AND host = 'some-host'" );
         Assert.Equal( 1, resultSet2.Results.Count );

         var result2 = resultSet2.Results[ 0 ];
         Assert.Equal( 0, result2.Series.Count );
         Assert.False( result2.Succeeded );
      }

      [Fact]
      public async Task Should_Create_And_Drop_Series_With_Null_Tags()
      {
         var state = new ComputerInfo
         {
            CPU = 0.42,
            RAM = 1024,
            Timestamp = DateTime.UtcNow - TimeSpan.FromMinutes( 5 ),
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "dmt3", new[] { state } );

         var resultSet1 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt3" );
         Assert.Equal( 1, resultSet1.Results.Count );

         var result1 = resultSet1.Results[ 0 ];
         Assert.Equal( 1, result1.Series.Count );

         var series1 = result1.Series[ 0 ];
         Assert.Equal( 1, series1.Rows.Count );

         await _client.DropSeries( InfluxClientFixture.DatabaseName, "dmt3" );

         var resultSet2 = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, "SELECT * FROM dmt3" );
         Assert.Equal( 1, resultSet2.Results.Count );

         var result2 = resultSet2.Results[ 0 ];
         Assert.Equal( 0, result2.Series.Count );
         Assert.False( result2.Succeeded );
      }

      [Fact]
      public async Task Should_Create_Show_Modify_And_Drop_Retention_Policy()
      {
         await _client.CreateRetentionPolicyAsync( InfluxClientFixture.DatabaseName, "dmt4RetentionPolicy", "1d", 1, false );

         var result = await _client.ShowRetentionPoliciesAsync( InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Name == "dmt4RetentionPolicy" );

         await _client.AlterRetentionPolicyAsync( InfluxClientFixture.DatabaseName, "dmt4RetentionPolicy", "4d", 1, false );
         
         await _client.DropRetentionPolicyAsync( InfluxClientFixture.DatabaseName, "dmt4RetentionPolicy" );
      }

      [Fact]
      public async Task Should_Throw_When_Dropping_Nonexisting_Retention_Policy()
      {
         await Assert.ThrowsAsync( typeof( InfluxException ), async () =>
          {
             await _client.DropRetentionPolicyAsync( InfluxClientFixture.DatabaseName, "dmt5RetentionPolicy" );
          } );
      }
   }
}
