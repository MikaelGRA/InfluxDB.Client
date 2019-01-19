using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Vibrant.InfluxDB.Client.Tests
{
   [Collection( "InfluxClient collection" )]
   public class LocalDateTimeOffsetParserTests
   {
      [Theory]
      [MemberData( nameof( ValidOffsetDataSource.TestData ), MemberType = typeof( ValidOffsetDataSource ) )]
      public void ToEpoch_WithMilisecondPrecision_Should_Be_Equivalent_To_Bcl( int offset )
      {
         var parser = new LocalDateTimeOffsetParser();

         var input = new DateTimeOffset( DateTime.UtcNow.Ticks, TimeSpan.FromHours( offset ) );

         var result = parser.ToEpoch( TimestampPrecision.Millisecond, input );
         
         var expected = input.ToUnixTimeMilliseconds();

         Assert.Equal( expected, result );
      }

      [Theory]
      [MemberData( nameof( ValidOffsetDataSource.TestData ), MemberType = typeof( ValidOffsetDataSource ) )]
      public void ToEpoch_WithSecondPrecision_Should_Be_Equivalent_To_Bcl( int offset )
      {
         var parser = new LocalDateTimeOffsetParser();

         var input = new DateTimeOffset( DateTime.UtcNow.Ticks, TimeSpan.FromHours( offset ) );

         var result = parser.ToEpoch( TimestampPrecision.Second, input );

         var expected = input.ToUnixTimeSeconds();

         Assert.Equal( expected, result );
      }

      [Theory]
      [MemberData( nameof( ValidOffsetDataSource.TestData ), MemberType = typeof( ValidOffsetDataSource ) )]
      public void ToEpoch_WithMilisecondPrecision_Should_Be_Equivalent_To_Bcl_Nullable( int offset )
      {
         var parser = new NullableLocalDateTimeOffsetParser();

         var input = new DateTimeOffset( DateTime.UtcNow.Ticks, TimeSpan.FromHours( offset ) );

         var result = parser.ToEpoch( TimestampPrecision.Millisecond, input );

         var expected = input.ToUnixTimeMilliseconds();

         Assert.Equal( expected, result );
      }

      [Theory]
      [MemberData( nameof( ValidOffsetDataSource.TestData ), MemberType = typeof( ValidOffsetDataSource ) )]
      public void ToEpoch_WithSecondPrecision_Should_Be_Equivalent_To_Bcl_Nullable( int offset )
      {
         var parser = new NullableLocalDateTimeOffsetParser();

         var input = new DateTimeOffset( DateTime.UtcNow.Ticks, TimeSpan.FromHours( offset ) );

         var result = parser.ToEpoch( TimestampPrecision.Second, input );

         var expected = input.ToUnixTimeSeconds();

         Assert.Equal( expected, result );
      }
   }

   public static class ValidOffsetDataSource
   {
      private static readonly List<object[]> _data = Enumerable.Range( -12, 14 ).Select( x => new object[] { (object)x } ).ToList();

      public static IEnumerable<object[]> TestData
      {
         get { return _data; }
      }
   }
}