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
   public class ReadWriteTests
   {
      private const string Unused = "unuseddatabasename";

      private InfluxClient _client;

      public ReadWriteTests( InfluxClientFixture fixture )
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

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Typed_Rows_To_Database( int rows )
      {
         var infos = CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos, TimestampPrecision.Nanosecond, Consistency.One );
      }

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Typed_Rows_With_Nulls_To_Database( int rows )
      {
         var infos = CreateTypedRowsStartingAt( new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, true );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos, TimestampPrecision.Nanosecond, Consistency.One );
      }

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Dynamic_Rows_To_Database( int rows )
      {
         var infos = CreateDynamicRowsStartingAt( new DateTime( 2012, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos, TimestampPrecision.Nanosecond, Consistency.One );
      }

      [Fact]
      public async Task Should_Write_All_Field_Types_To_Database()
      {
         var row = new VariationRow
         {
            Timestamp = new DateTime( 2013, 1, 1, 1, 1, 1, DateTimeKind.Utc ),
            Count = 1337,
            Indicator = true,
            Message = "Hello there\nWhat's up?",
            Percent = 0.37,
            Type = "tag Value",
            Category = TestEnum1.Value2,
            CategoryTag = TestEnum2.Value3,
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "variation", new[] { row }, TimestampPrecision.Nanosecond, Consistency.One );

         var resultSet = await _client.ReadAsync<VariationRow>( "SELECT * FROM variation", InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 1, series.Rows.Count );
      }

      [Fact]
      public async Task Should_Write_And_Query_Typed_Data()
      {
         var start = new DateTime( 2013, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = CreateTypedRowsStartingAt( start, 500, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos, TimestampPrecision.Nanosecond, Consistency.One );


         var from = start;
         var to = from.AddSeconds( 250 );

         var resultSet = await _client.ReadAsync<ComputerInfo>( $"SELECT * FROM computerInfo WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'", InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 250, series.Rows.Count );
      }
   }
}
