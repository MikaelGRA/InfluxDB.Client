using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Precision of DateTimes that are read or written to InfluxDB.
   /// </summary>
   public enum TimestampPrecision
   {
      /// <summary>
      /// Nanosecond precision.
      /// </summary>
      Nanosecond,

      /// <summary>
      /// Microsecond precision.
      /// </summary>
      Microsecond,

      /// <summary>
      /// Millisecond precision.
      /// </summary>
      Millisecond,

      /// <summary>
      /// Second precision.
      /// </summary>
      Second,

      /// <summary>
      /// Minute precision.
      /// </summary>
      Minute,

      /// <summary>
      /// Hour precision.
      /// </summary>
      Hours
   }
}
