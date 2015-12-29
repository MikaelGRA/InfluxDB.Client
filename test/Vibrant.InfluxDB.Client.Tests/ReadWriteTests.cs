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

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Typed_Rows_To_Database( int rows )
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos );
      }

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Typed_Rows_With_Nulls_To_Database( int rows )
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, true );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos );
      }

      [Theory]
      [InlineData( 500 )]
      [InlineData( 1000 )]
      [InlineData( 20000 )]
      public async Task Should_Write_Dynamic_Rows_To_Database( int rows )
      {
         var infos = InfluxClientFixture.CreateDynamicRowsStartingAt( new DateTime( 2012, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos );
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
            OtherTimestamp = new DateTime( 2011, 4, 23, 1, 23, 54, DateTimeKind.Utc ),
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "variation", new[] { row } );

         var resultSet = await _client.ReadAsync<VariationRow>( InfluxClientFixture.DatabaseName, "SELECT * FROM variation" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 1, series.Rows.Count );

         Assert.Equal( row, series.Rows[ 0 ] );
      }

      [Fact]
      public async Task Should_Write_And_Query_Typed_Data()
      {
         var start = new DateTime( 2013, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 500, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo", infos );


         var from = start;
         var to = from.AddSeconds( 250 );

         var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM computerInfo WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 250, series.Rows.Count );
      }

      [Fact]
      public async Task Should_Write_Type_With_Null_Timestamp()
      {
         var row = new SimpleRow
         {
            Value = 13.37
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "simpleRow", new[] { row } );

         var resultSet = await _client.ReadAsync<SimpleRow>( InfluxClientFixture.DatabaseName, "SELECT * FROM simpleRow" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 1, series.Rows.Count );

         Assert.Equal( row.Value, series.Rows[ 0 ].Value );
      }

      [Fact]
      public async Task Should_Write_And_Query_Grouped_Data()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo", infos );

         var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo GROUP BY region" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         foreach ( var region in InfluxClientFixture.Regions )
         {
            var kvp = new KeyValuePair<string, object>( "region", region );
            var group = result.FindGroup( new[] { kvp } );
            Assert.NotNull( group );
         }
      }

      [Fact]
      public async Task Should_Write_And_Query_Grouped_On_Enumerated_Data()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateEnumeratedRowsStartingAt( start, 5000 );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedEnumeratedRows", infos );

         var resultSet = await _client.ReadAsync<EnumeratedRow>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedEnumeratedRows GROUP BY type" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         foreach ( var type in InfluxClientFixture.TestEnums )
         {
            var kvp = new KeyValuePair<string, object>( "type", type );
            var group = result.FindGroup( new[] { kvp } );
            Assert.NotNull( group );
         }
      }
   }
}
