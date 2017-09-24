using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   internal static class TimestampPrecisionExtensions
   {
      internal static string GetQueryParameter( this TimestampPrecision that )
      {
         switch ( that )
         {
            case TimestampPrecision.Nanosecond:
               return "ns";
            case TimestampPrecision.Microsecond:
               return "u";
            case TimestampPrecision.Millisecond:
               return "ms";
            case TimestampPrecision.Second:
               return "s";
            case TimestampPrecision.Minute:
               return "m";
            case TimestampPrecision.Hours:
               return "h";
            default:
               throw new ArgumentException( "Invalid parameter value.", nameof( that ) );
         }
      }
   }
}
