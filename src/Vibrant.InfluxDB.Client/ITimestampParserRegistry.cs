namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// TimestampParserRegistry allowing for looking up a parser for a specific timestamp type.
   /// </summary>
   public interface ITimestampParserRegistry
   {
      /// <summary>
      /// Finds the timestamp parser for the specified timestamp type.
      /// </summary>
      /// <typeparam name="TTimestamp"></typeparam>
      /// <returns></returns>
      ITimestampParser<TTimestamp> FindTimestampParser<TTimestamp>();

      /// <summary>
      /// Adds or replaces the timestamp parser for the specified timestamp type.
      /// </summary>
      /// <typeparam name="TTimestamp"></typeparam>
      /// <typeparam name="TTimestampParser"></typeparam>
      /// <param name="timestampParser"></param>
      void AddOrReplace<TTimestamp, TTimestampParser>( TTimestampParser timestampParser ) where TTimestampParser : ITimestampParser<TTimestamp>;

      /// <summary>
      /// Removes the timestamp parser for the specified timestamp type.
      /// </summary>
      /// <typeparam name="TTimestamp"></typeparam>
      void Remove<TTimestamp>();

      /// <summary>
      /// Checks if a timestamp parser is registered for the specified timestamp type.
      /// </summary>
      /// <typeparam name="TTimestamp"></typeparam>
      /// <returns></returns>
      bool Contains<TTimestamp>();
   }
}
