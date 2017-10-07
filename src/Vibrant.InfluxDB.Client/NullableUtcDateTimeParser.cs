using System;
using System.Globalization;
using Vibrant.InfluxDB.Client.Resources;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Implementation of ITimestampParser that always parses to UTC DateTimes.
   /// </summary>
   public class NullableUtcDateTimeParser : ITimestampParser<DateTime?>
   {
      private static readonly DateTimeStyles OnlyUtcStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

      /// <inheritdoc />
      public long ToEpoch( TimestampPrecision? precision, DateTime? timestamp )
      {
         return timestamp.Value.ToPrecision( precision.Value );
      }

      /// <inheritdoc />
      public DateTime? ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString )
      {
         if( epochTimeLongOrIsoTimestampString is string )
         {
            return DateTime.Parse( (string)epochTimeLongOrIsoTimestampString, CultureInfo.InvariantCulture, OnlyUtcStyles );
         }
         else if( epochTimeLongOrIsoTimestampString is long )
         {
            return DateTimeExtensions.FromEpochTime( (long)epochTimeLongOrIsoTimestampString, precision.Value );
         }

         throw new InfluxException( string.Format( Errors.CouldNotParseTimestamp, epochTimeLongOrIsoTimestampString ) );
      }
   }
}
