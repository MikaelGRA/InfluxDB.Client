using System;
using System.Globalization;
using Vibrant.InfluxDB.Client.Resources;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Implementation of ITimestampParser that maintains the offset from UTC.
   /// </summary>
   public class LocalDateTimeOffsetParser : ITimestampParser<DateTimeOffset>
   {
      /// <inheritdoc />
      public long ToEpoch( TimestampPrecision precision, DateTimeOffset timestamp )
      {
         return timestamp.ToPrecision( precision );
      }

      /// <inheritdoc />
      public DateTimeOffset ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString )
      {
         if( !precision.HasValue )
         {
            // if no precision is specified, the time column is returned as a ISO8601-timestamp.
            return DateTimeOffset.Parse( (string)epochTimeLongOrIsoTimestampString, CultureInfo.InvariantCulture );
         }
         else
         {
            // the offset cannot be preserved with an epoch time, will throw to alert user
            throw new InfluxException( Errors.MissingOffsetInEpochTime );
         }
      }
   }
}
