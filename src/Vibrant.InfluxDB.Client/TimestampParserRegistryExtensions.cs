namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Extensions for ITimestampParserRegistry.
   /// </summary>
   public static class TimestampParserRegistryExtensions
   {
      internal static ITimestampParser<TTimestamp> FindTimestampParserOrNull<TTimestamp>( this ITimestampParserRegistry registry )
      {
         if( typeof( TTimestamp ) == typeof( NullTimestamp ) )
            return null;

         return registry.FindTimestampParser<TTimestamp>();
      }
      
      /// <summary>
      /// Adds or replaces a TimestampParser for the specified timestamp type.
      /// </summary>
      /// <typeparam name="TTimestamp"></typeparam>
      /// <typeparam name="TTimestampParser"></typeparam>
      /// <param name="registry"></param>
      public static void AddOrReplace<TTimestamp, TTimestampParser>( this ITimestampParserRegistry registry )
         where TTimestampParser : ITimestampParser<TTimestamp>, new()
      {
         registry.AddOrReplace<TTimestamp, TTimestampParser>( new TTimestampParser() );
      }
   }
}
