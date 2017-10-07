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
      public long ToEpoch( TimestampPrecision? precision, DateTime timestamp )
      {
         return timestamp.ToPrecision( precision.Value );
      }

      /// <inheritdoc />
      public DateTime ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString )
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
