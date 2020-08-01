namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// ITimestampParser is responsible for parsing the 'time' column
   /// of data returned, allowing use of custom DateTime types.
   /// </summary>
   /// <typeparam name="TTimestamp"></typeparam>
   public interface ITimestampParser<TTimestamp>
   {
      /// <summary>
      /// Parses a epoch time (UTC) or ISO8601-timestamp (potentially with offset) to a date and time.
      /// This is used when reading data from influxdb.
      /// </summary>
      /// <param name="precision">TimestampPrecision provided by the current InfluxQueryOptions.</param>
      /// <param name="epochTimeLongOrIsoTimestampString">The raw value returned by the query.</param>
      /// <returns>The parsed timestamp.</returns>
      TTimestamp ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString );

      /// <summary>
      /// Converts the timestamp to epoch time (UTC). This is used when writing data to influxdb.
      /// </summary>
      /// <param name="precision">TimestampPrecision provided by the current InfluxWriteOptions.</param>
      /// <param name="timestamp">The timestamp to convert.</param>
      /// <returns>The UTC epoch time.</returns>
      long ToEpoch( TimestampPrecision precision, TTimestamp timestamp );
   }
}
