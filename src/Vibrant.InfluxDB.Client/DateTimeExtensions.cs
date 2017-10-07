using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Some simple DateTimeExxtensions that makes it easier to work with InfluxDB.
   /// </summary>
   public static class DateTimeExtensions
   {
      private static readonly DateTime Epoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
      private static readonly long TicksPerMicrosecond = 10;

      internal static DateTime FromEpochTime( long ticks, TimestampPrecision precision )
      {
         switch( precision )
         {
            case TimestampPrecision.Nanosecond:
               return Epoch.AddTicks( ticks / 100 );
            case TimestampPrecision.Microsecond:
               return Epoch.AddTicks( ticks * TicksPerMicrosecond );
            case TimestampPrecision.Millisecond:
               return Epoch.AddMilliseconds( ticks );
            case TimestampPrecision.Second:
               return Epoch.AddSeconds( ticks );
            case TimestampPrecision.Minute:
               return Epoch.AddMinutes( ticks );
            case TimestampPrecision.Hours:
               return Epoch.AddHours( ticks );
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( precision ) );
         }
      }

      /// <summary>
      /// Returns a long representing the number of ticks (in the given precision) the TimeSpan represents.
      /// </summary>
      /// <param name="that"></param>
      /// <param name="precision"></param>
      /// <returns></returns>
      public static long ToPrecision( this TimeSpan that, TimestampPrecision precision )
      {
         switch( precision )
         {
            case TimestampPrecision.Nanosecond:
               return that.Ticks * 100;
            case TimestampPrecision.Microsecond:
               return that.Ticks / TicksPerMicrosecond;
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
         switch( precision )
         {
            case TimestampPrecision.Nanosecond:
               return ( that - Epoch ).Ticks * 100;
            case TimestampPrecision.Microsecond:
               return ( that - Epoch ).Ticks / TicksPerMicrosecond;
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
      /// Returns a long representing the number of ticks (in the given precision) the DateTime is from 1. Jan 1970 (UTC).
      /// </summary>
      /// <param name="that"></param>
      /// <param name="precision"></param>
      /// <returns></returns>
      public static long ToPrecision( this DateTimeOffset that, TimestampPrecision precision )
      {
         switch( precision )
         {
            case TimestampPrecision.Nanosecond:
               return ( that - Epoch + that.Offset ).Ticks * 100;
            case TimestampPrecision.Microsecond:
               return ( that - Epoch + that.Offset ).Ticks / TicksPerMicrosecond;
            case TimestampPrecision.Millisecond:
               return ( that - Epoch + that.Offset ).Ticks / TimeSpan.TicksPerMillisecond;
            case TimestampPrecision.Second:
               return ( that - Epoch + that.Offset ).Ticks / TimeSpan.TicksPerSecond;
            case TimestampPrecision.Minute:
               return ( that - Epoch + that.Offset ).Ticks / TimeSpan.TicksPerMinute;
            case TimestampPrecision.Hours:
               return ( that - Epoch + that.Offset ).Ticks / TimeSpan.TicksPerHour;
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( precision ) );
         }
      }

      /// <summary>
      /// Returns a string representing a influx timespan.
      /// </summary>
      /// <param name="that"></param>
      /// <param name="requireSingleUnit"></param>
      /// <returns></returns>
      public static string ToInfluxTimeSpan( this TimeSpan that, bool requireSingleUnit )
      {
         if( that.Ticks > -10 && that.Ticks < 10 )
         {
            throw new InvalidOperationException( "A timespan cannot be less than 1 microsecond." );
         }

         var duration = that.Duration();

         var sb = new StringBuilder();
         if( that < TimeSpan.Zero )
         {
            sb.Append( "-" );
         }

         bool hasDigits = false;

         var d = duration.Days;
         if( d != 0 )
         {
            if( d >= 7 )
            {
               var w = d / 7;
               if( w != 0 )
               {
                  sb.Append( w ).Append( "w" );
                  if( hasDigits && requireSingleUnit )
                  {
                     throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
                  }
                  hasDigits = true;
               }

               d %= 7;
            }

            if( d != 0 )
            {
               sb.Append( d ).Append( "d" );
               if( hasDigits && requireSingleUnit )
               {
                  throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
               }
               hasDigits = true;
            }
         }

         var h = duration.Hours;
         if( h != 0 )
         {
            sb.Append( h ).Append( "h" );
            if( hasDigits && requireSingleUnit )
            {
               throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
            }
            hasDigits = true;
         }

         var m = duration.Minutes;
         if( m != 0 )
         {
            sb.Append( m ).Append( "m" );
            if( hasDigits && requireSingleUnit )
            {
               throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
            }
            hasDigits = true;
         }

         var s = duration.Seconds;
         if( s != 0 )
         {
            sb.Append( s ).Append( "s" );
            if( hasDigits && requireSingleUnit )
            {
               throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
            }
            hasDigits = true;
         }

         var u = ( duration.Ticks / 10 ) % 1000;
         if( u != 0 )
         {
            sb.Append( u ).Append( "u" );
            if( hasDigits && requireSingleUnit )
            {
               throw new NotSupportedException( "The specfied timespan must resolve to a single full unit (week, day, minute, second and microsecond)." );
            }
            hasDigits = true;
         }

         return sb.ToString();
      }

      /// <summary>
      /// Gets a string that can be used as part of a query to InfluxDB.
      /// </summary>
      /// <param name="that"></param>
      /// <returns></returns>
      public static string ToIso8601( this DateTime that )
      {
         if( that.Kind == DateTimeKind.Local )
         {
            that = that.ToUniversalTime();
         }

         return that.ToString( "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture );
      }
   }
}