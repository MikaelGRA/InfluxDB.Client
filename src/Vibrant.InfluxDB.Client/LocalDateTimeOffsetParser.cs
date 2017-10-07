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
      public long ToEpoch( TimestampPrecision? precision, DateTimeOffset timestamp )
      {
         return timestamp.ToPrecision( precision.Value );
      }

      /// <inheritdoc />
      public DateTimeOffset ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString )
      {
         if( epochTimeLongOrIsoTimestampString is string )
         {
            return DateTimeOffset.Parse( (string)epochTimeLongOrIsoTimestampString, CultureInfo.InvariantCulture );
         }
         else if( epochTimeLongOrIsoTimestampString is long )
         {
            throw new InfluxException( Errors.MissingOffsetInEpochTime );
         }

         throw new InfluxException( string.Format( Errors.CouldNotParseTimestamp, epochTimeLongOrIsoTimestampString ) );
      }
   }
}
