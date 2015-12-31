using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Some simple DateTimeExxtensions that makes it easier to work with InfluxDB.
   /// </summary>
   public static class DateTimeExtensions
   {
      private static readonly DateTime Epoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

      /// <summary>
      /// Returns a long representing the number of ticks (in the given precision) the TimeSpan represents.
      /// </summary>
      /// <param name="that"></param>
      /// <param name="precision"></param>
      /// <returns></returns>
      public static long ToPrecision( this TimeSpan that, TimestampPrecision precision )
      {
         switch ( precision )
         {
            case TimestampPrecision.Nanosecond:
               return that.Ticks * 100;
            case TimestampPrecision.Microsecond:
               return that.Ticks / 10;
            case TimestampPrecision.Millisecond:
               return that.Ticks / TimeSpan.TicksPerMillisecond;
            case TimestampPrecision.Second:
               return that.Ticks / TimeSpan.TicksPerSecond;
            case TimestampPrecision.Minute:
               return that.Ticks / TimeSpan.TicksPerMinute;
            case TimestampPrecision.Hours:
               return that.Ticks / TimeSpan.TicksPerHour;
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( precision ) );
         }
      }

      /// <summary>
      /// Returns a long representing the number of ticks (in the given precision) the DateTime is from 1. Jan 1970.
      /// </summary>
      /// <param name="that"></param>
      /// <param name="precision"></param>
      /// <returns></returns>
      public static long ToPrecision( this DateTime that, TimestampPrecision precision )
      {
         switch ( precision )
         {
            case TimestampPrecision.Nanosecond:
               return ( that - Epoch ).Ticks * 100;
            case TimestampPrecision.Microsecond:
               return ( that - Epoch ).Ticks / 10;
            case TimestampPrecision.Millisecond:
               return ( that - Epoch ).Ticks / TimeSpan.TicksPerMillisecond;
            case TimestampPrecision.Second:
               return ( that - Epoch ).Ticks / TimeSpan.TicksPerSecond;
            case TimestampPrecision.Minute:
               return ( that - Epoch ).Ticks / TimeSpan.TicksPerMinute;
            case TimestampPrecision.Hours:
               return ( that - Epoch ).Ticks / TimeSpan.TicksPerHour;
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( precision ) );
         }
      }

      /// <summary>
      /// Gets a string that can be used as part of a query to InfluxDB.
      /// </summary>
      /// <param name="that"></param>
      /// <returns></returns>
      public static string ToIso8601( this DateTime that )
      {
         if ( that.Kind == DateTimeKind.Local )
         {
            that = that.ToUniversalTime();
         }

         return that.ToString( "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture );
      }
   }
}