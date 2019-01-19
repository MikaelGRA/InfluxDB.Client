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
   public class SchemaExplorationTests
   {
      private InfluxClient _client;

      public SchemaExplorationTests( InfluxClientFixture fixture )
      {
         _client = fixture.Client;
      }

      [Fact]
      public async Task Should_Show_Series()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set1Measurement", infos );

         var result = await _client.ShowSeriesAsync( InfluxClientFixture.DatabaseName, "set1Measurement" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         foreach( var key in InfluxClientFixture.GenerateShowSeriesKeysFor( "set1Measurement" ) )
         {
            Assert.Contains( key, series.Rows.Select( x => x.Key ) );
         }
      }

      [Fact]
      public async Task Should_Show_Measurements()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 200, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set2Measurement", infos );

         var result = await _client.ShowMeasurementsAsync( InfluxClientFixture.DatabaseName );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Name == "set2Measurement" );
      }

      [Fact]
      public async Task Should_Show_Tag_Keys()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2012, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set3Measurement", infos );

         var result = await _client.ShowTagKeysAsync( InfluxClientFixture.DatabaseName, "set3Measurement" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.TagKey == "host" );
         Assert.Contains( series.Rows, x => x.TagKey == "region" );
      }

      [Fact]
      public async Task Should_Show_Field_Keys()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2013, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set4Measurement", infos );

         var result = await _client.ShowFieldKeysAsync( InfluxClientFixture.DatabaseName, "set4Measurement" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.FieldKey == "cpu" );
         Assert.Contains( series.Rows, x => x.FieldKey == "ram" );
      }

      [Fact]
      public async Task Should_Show_Tag_Values()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2014, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 5000, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set5Measurement", infos );

         var result = await _client.ShowTagValuesAsync( InfluxClientFixture.DatabaseName, "region", "set5Measurement" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         foreach ( var region in InfluxClientFixture.Regions )
         {
            Assert.Contains( series.Rows, x => x.Value == region );
         }
      }

      [Fact]
      public async Task Should_Show_Measurements_With_Measurement()
      {
         var infos = InfluxClientFixture.CreateTypedRowsStartingAt( new DateTime( 2015, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 200, false );
         await _client.WriteAsync( InfluxClientFixture.DatabaseName, "set6Measurement", infos );

         var result = await _client.ShowMeasurementsWithMeasurementAsync( InfluxClientFixture.DatabaseName, "/set6.*/" );
         Assert.Equal( 1, result.Series.Count );

         var series = result.Series[ 0 ];
         Assert.Contains( series.Rows, x => x.Name == "set6Measurement" );
      }
   }
}
