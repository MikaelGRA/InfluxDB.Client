using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Resources;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Implementation of ITimestampParser that always parses to UTC DateTimes.
   /// </summary>
   public class UtcDateTimeParser : ITimestampParser<DateTime>
   {
      private static readonly DateTimeStyles OnlyUtcStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

      /// <inheritdoc />
      public long ToEpoch( TimestampPrecision precision, DateTime timestamp )
      {
         return timestamp.ToPrecision( precision );
      }

      /// <inheritdoc />
      public DateTime ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString )
      {
         if( !precision.HasValue || epochTimeLongOrIsoTimestampString is string )
         {
            // if no precision is specified, the time column is returned as a ISO8601-timestamp.
            return DateTime.Parse( (string)epochTimeLongOrIsoTimestampString, CultureInfo.InvariantCulture, OnlyUtcStyles );
         }
         else
         {
            // if a precision is specified, the time column is returned as a long epoch time (accuracy based on precision)
            return DateTimeExtensions.FromEpochTime( (long)epochTimeLongOrIsoTimestampString, precision.Value );
         }
      }
   }
}
