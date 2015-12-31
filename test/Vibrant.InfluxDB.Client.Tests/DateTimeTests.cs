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
   public class DateTimeTests
   {
      private InfluxClient _client;

      public DateTimeTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Read_And_Write_Typed_DateTimes()
      {
         var time = new DateTime( 2013, 1, 1, 0, 0, 0, DateTimeKind.Utc );

         var row = new DateTimeRow
         {
            Timestamp = time,
            OtherTime = time,
            OtherTimeAsString = time.ToIso8601()
         };

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "dateTimeTests", new[] { row } );

         var resultSet = await _client.ReadAsync<DateTimeRow>( InfluxClientFixture.DatabaseName, "SELECT * FROM dateTimeTests" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 1, series.Rows.Count );

         Assert.Equal( row.OtherTime, series.Rows[ 0 ].OtherTime );
         Assert.Equal( row.Timestamp, series.Rows[ 0 ].Timestamp );
         Assert.Equal( row.OtherTimeAsString, series.Rows[ 0 ].OtherTimeAsString );
      }

      [Fact]
      public async Task Should_Read_And_Write_Dynamic_DateTimes()
      {
         var time = new DateTime( 2013, 1, 1, 0, 0, 0, DateTimeKind.Utc );

         var row = new DynamicInfluxRow();
         row.Timestamp = time;
         row.Fields.Add( "otherTime", time );
         row.Fields.Add( "otherTimeAsString", time.ToIso8601() );

         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "dateTimeTests2", new[] { row } );

         var resultSet = await _client.ReadAsync<DynamicInfluxRow>( InfluxClientFixture.DatabaseName, "SELECT * FROM dateTimeTests2" );
         Assert.Equal( 1, resultSet.Results.Count );

         var result = resultSet.Results[ 0 ];
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Equal( 1, series.Rows.Count );

         Assert.Equal( time.ToIso8601(), series.Rows[ 0 ].Fields[ "otherTime" ] );
         Assert.Equal( row.Timestamp, series.Rows[ 0 ].Timestamp );
         Assert.Equal( time.ToIso8601(), series.Rows[ 0 ].Fields[ "otherTimeAsString" ] );
      }
   }
}
