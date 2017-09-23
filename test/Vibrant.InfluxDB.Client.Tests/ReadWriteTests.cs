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

      [Fact]
      public async Task Should_Write_Typed_Rows_To_Database_With_Chunking()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 20000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo1", infos );

         var resultSet = await _client.ReadAsync<ComputerInfo>(
            InfluxClientFixture.DatabaseName,
            "select * from computerInfo1",
            new InfluxQueryOptions { ChunkSize = 1000 } );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 20000, series.Rows.Count );
      }

      [Theory]
      [InlineData( 500, 2011, "computerInfo2_500" )]
      [InlineData( 1000, 2012, "computerInfo2_1000" )]
      [InlineData( 20000, 2013, "computerInfo2_20000" )]
      public async Task Should_Write_Typed_Rows_To_Database( int rows, int startYear, string tableName )
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( startYear, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, tableName, infos );

         var secondResultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"select * from {tableName}" );

         var result = secondResultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( rows, series.Rows.Count );
      }

      [Theory]
      [InlineData( 500, 2011 )]
      [InlineData( 1000, 2012 )]
      [InlineData( 20000, 2013 )]
      public async Task Should_Write_Typed_Rows_With_Nulls_To_Database( int rows, int startYear )
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( startYear, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows, true );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo3", infos );
      }

      [Theory]
      [InlineData( 500, 2011 )]
      [InlineData( 1000, 2012 )]
      [InlineData( 20000, 2013 )]
      public async Task Should_Write_Dynamic_Rows_To_Database( int rows, int startYear )
      {
         var infos = InfluxClientFixture.CreateDynamicRowsStartingAt( new DateTime( startYear, 1, 1, 1, 1, 1, DateTimeKind.Utc ), rows );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo4", infos );
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
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo5", infos );


         var from = start;
         var to = from.AddSeconds( 250 );

         var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM computerInfo5 WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 250, series.Rows.Count );

         // attempt deletion
         await _client.DeleteRangeAsync( InfluxClientFixture.DatabaseName, "computerInfo5", from, to );

         resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM computerInfo5 WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
         Assert.Equal( 1, resultSet.Results.Count );

         result = resultSet.Results[ 0 ];
         Assert.Equal( 0, result.Series.Count );
      }

      //[Fact]
      //public async Task Should_Write_And_Query_Typed_Data_Using_Post()
      //{
      //   var start = new DateTime( 2013, 1, 1, 1, 1, 1, DateTimeKind.Utc );
      //   var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 500, false );
      //   await _client.WriteAsync( InfluxClientFixture.DatabaseName, "computerInfo6", infos );
      //   var options = new InfluxQueryOptions
      //   {
      //      UsePost = true
      //   };

      //   var from = start;
      //   var to = from.AddSeconds( 250 );

      //   var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM computerInfo6 WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'", options );
      //   Assert.Equal( 1, resultSet.Results.Count );

      //   var result = resultSet.Results[ 0 ];
      //   Assert.Equal( 1, result.Series.Count );

      //   var series = result.Series[ 0 ];
      //   Assert.Equal( 250, series.Rows.Count );

      //   // attempt deletion
      //   await _client.DeleteRangeAsync( InfluxClientFixture.DatabaseName, "computerInfo6", from, to );

      //   resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM computerInfo6 WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'", options );
      //   Assert.Equal( 1, resultSet.Results.Count );

      //   result = resultSet.Results[ 0 ];
      //   Assert.Equal( 0, result.Series.Count );
      //}

      [Fact]
      public async Task Should_Write_Read_And_Delete_Typed_Data()
      {
         for( int i = 0 ; i < 2 ; i++ )
         {
            var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
            var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 250, false );
            await _client.WriteAsync( InfluxClientFixture.DatabaseName, "otherData", infos );

            var from = start;
            var to = from.AddSeconds( 250 );

            var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM otherData WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
            Assert.Equal( 1, resultSet.Results.Count );

            var result = resultSet.Results[ 0 ];
            Assert.Equal( 1, result.Series.Count );

            var series = result.Series[ 0 ];
            Assert.Equal( 250, series.Rows.Count );

            // attempt deletion
            await _client.DeleteRangeAsync( InfluxClientFixture.DatabaseName, "otherData", from, to );

            resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM otherData WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
            Assert.Equal( 1, resultSet.Results.Count );

            result = resultSet.Results[ 0 ];
            Assert.Equal( 0, result.Series.Count );
         }
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
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo1", infos );

         var resultSet = await _client.ReadAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo1 GROUP BY region" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         foreach( var region in InfluxClientFixture.Regions )
         {
            var kvp = new KeyValuePair<string, object>( "region", region );
            var group = result.FindGroup( "groupedComputerInfo1", new[] { kvp } );
            Assert.NotNull( group );
         }
      }

      [Fact]
      public async Task Should_Write_And_Query_Grouped_Data_With_Chunking()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo2", infos );

         var resultSet = await _client.ReadAsync<ComputerInfo>(
            InfluxClientFixture.DatabaseName,
            $"SELECT * FROM groupedComputerInfo2 GROUP BY region",
            new InfluxQueryOptions { ChunkSize = 100 } );

         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         foreach( var region in InfluxClientFixture.Regions )
         {
            var kvp = new KeyValuePair<string, object>( "region", region );
            var group = result.FindGroup( "groupedComputerInfo2", new[] { kvp } );
            Assert.NotNull( group );
         }
      }

      [Fact]
      public async Task Should_Write_And_Query_Grouped_Data_Using_Computed_Columns()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         var end = infos.Last().Timestamp;
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo3", infos );

         var resultSet = await _client.ReadAsync<ComputedComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT MEAN(cpu) AS cpu, COUNT(ram) AS ram FROM groupedComputerInfo3 WHERE time >= '{start.ToIso8601()}' AND time <= '{end.ToIso8601()}' GROUP BY time(1ms), region fill(none)" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         foreach( var region in InfluxClientFixture.Regions )
         {
            var kvp = new KeyValuePair<string, object>( "region", region );
            var group = result.FindGroup( "groupedComputerInfo3", new[] { kvp } );
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
         foreach( var type in InfluxClientFixture.TestEnums )
         {
            var kvp = new KeyValuePair<string, object>( "type", type );
            var group = result.FindGroup( "groupedEnumeratedRows", new[] { kvp } );
            Assert.NotNull( group );
         }
      }

      [Theory]
      [InlineData( 100, 'A' )]
      [InlineData( 200, 'B' )]
      [InlineData( 5000, 'C' )]
      public async Task Should_Write_And_Query_Deferred_Grouped_Data_With_Multi_Query( int count, char c )
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, count, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, $"groupedComputerInfo4{c}", infos );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, $"groupedComputerInfo5{c}", infos );

         var chunkedResultSet = await _client.ReadChunkedAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo4{c} GROUP BY region;SELECT * FROM groupedComputerInfo5{c} GROUP BY region", new InfluxQueryOptions { ChunkSize = 200 } );
         
         InfluxChunkedResult<ComputerInfo> currentResult;
         InfluxChunkedSeries<ComputerInfo> currentSerie;
         InfluxChunk<ComputerInfo> currentChunk;
         int resultCount = 0;
         int serieCount = 0;
         int rowCount = 0;

         using( chunkedResultSet )
         {
            while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
            {
               while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
               {
                  while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
                  {
                     rowCount += currentChunk.Rows.Count;
                  }
                  serieCount++;
               }
               resultCount++;
            }
         }

         Assert.Equal( 1 * 2, resultCount );
         Assert.Equal( InfluxClientFixture.Regions.Length * 2, serieCount );
         Assert.Equal( count * 2, rowCount );
      }

      [Fact]
      public async Task Should_Write_And_Query_Deferred_Grouped_Data()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo6", infos );

         var chunkedResultSet = await _client.ReadChunkedAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo6 GROUP BY region", new InfluxQueryOptions { ChunkSize = 200 } );

         InfluxChunkedResult<ComputerInfo> currentResult;
         InfluxChunkedSeries<ComputerInfo> currentSerie;
         InfluxChunk<ComputerInfo> currentChunk;
         int resultCount = 0;
         int serieCount = 0;
         int rowCount = 0;

         HashSet<DateTime> times = new HashSet<DateTime>();
         using( chunkedResultSet )
         {
            while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
            {
               while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
               {
                  while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
                  {
                     foreach( var row in currentChunk.Rows )
                     {
                        Assert.False( times.Contains( row.Timestamp ) );
                        times.Add( row.Timestamp );
                     }
                     rowCount += currentChunk.Rows.Count;
                  }
                  serieCount++;
               }
               resultCount++;
            }
         }

         Assert.Equal( 1, resultCount );
         Assert.Equal( InfluxClientFixture.Regions.Length, serieCount );
         Assert.Equal( 5000, rowCount );
      }

      [Fact]
      public async Task Should_Write_And_Query_Deferred_Data()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo7", infos );

         var chunkedResultSet = await _client.ReadChunkedAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo7", new InfluxQueryOptions { ChunkSize = 200 } );

         InfluxChunkedResult<ComputerInfo> currentResult;
         InfluxChunkedSeries<ComputerInfo> currentSerie;
         InfluxChunk<ComputerInfo> currentChunk;
         int resultCount = 0;
         int serieCount = 0;
         int rowCount = 0;

         HashSet<DateTime> times = new HashSet<DateTime>();
         using( chunkedResultSet )
         {
            while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
            {
               while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
               {
                  while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
                  {
                     foreach( var row in currentChunk.Rows )
                     {
                        Assert.False( times.Contains( row.Timestamp ) );
                        times.Add( row.Timestamp );
                     }
                     rowCount += currentChunk.Rows.Count;
                  }
                  serieCount++;
               }
               resultCount++;
            }
         }

         Assert.Equal( 1, resultCount );
         Assert.Equal( 1, serieCount );
         Assert.Equal( 5000, rowCount );
      }

      [Fact]
      public async Task Should_Write_And_Query_Deferred_Grouped_Data_Breaking()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo8", infos );

         var chunkedResultSet = await _client.ReadChunkedAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo8 GROUP BY region", new InfluxQueryOptions { ChunkSize = 200 } );

         InfluxChunkedResult<ComputerInfo> currentResult;
         InfluxChunkedSeries<ComputerInfo> currentSerie;
         InfluxChunk<ComputerInfo> currentChunk;
         int resultCount = 0;
         int serieCount = 0;
         int rowCount = 0;

         using( chunkedResultSet )
         {
            while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
            {
               while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
               {
                  while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
                  {
                     rowCount += currentChunk.Rows.Count;
                     break;
                  }
                  serieCount++;
               }
               resultCount++;
            }
         }

         Assert.Equal( 1, resultCount );
         Assert.Equal( InfluxClientFixture.Regions.Length, serieCount );
         Assert.Equal( 1000, rowCount );
      }

      [Fact]
      public async Task Should_Write_And_Query_Deferred_Grouped_Data_Breaking_2()
      {
         var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo9", infos );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo10", infos );

         var chunkedResultSet = await _client.ReadChunkedAsync<ComputerInfo>( InfluxClientFixture.DatabaseName, $"SELECT * FROM groupedComputerInfo9 GROUP BY region;SELECT * FROM groupedComputerInfo10 GROUP BY region", new InfluxQueryOptions { ChunkSize = 200 } );

         InfluxChunkedResult<ComputerInfo> currentResult;
         InfluxChunkedSeries<ComputerInfo> currentSerie;
         InfluxChunk<ComputerInfo> currentChunk;
         int resultCount = 0;
         int serieCount = 0;
         int rowCount = 0;
         HashSet<string> measurements = new HashSet<string>();

         using( chunkedResultSet )
         {
            while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
            {
               while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
               {
                  while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
                  {
                     rowCount += currentChunk.Rows.Count;
                     break;
                  }
                  measurements.Add( currentSerie.Name );
                  serieCount++;
                  break;
               }
               resultCount++;
            }
         }

         Assert.Equal( 2, resultCount );
         Assert.Equal( 2, serieCount );
         Assert.Equal( 400, rowCount );
         Assert.True( measurements.Contains( "groupedComputerInfo9" ) );
         Assert.True( measurements.Contains( "groupedComputerInfo10" ) );
      }
   }
}
