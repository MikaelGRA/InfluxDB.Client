using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Resources;

namespace Vibrant.InfluxDB.Client
{
   internal class DefaultTimestampParserRegistry : ITimestampParserRegistry
   {
      private readonly Dictionary<Type, object> _timestampParsers;

      public DefaultTimestampParserRegistry()
      {
         _timestampParsers = new Dictionary<Type, object>();

         AddOrReplace<DateTime, UtcDateTimeParser>( new UtcDateTimeParser() );
         AddOrReplace<DateTime?, NullableUtcDateTimeParser>( new NullableUtcDateTimeParser() );
         AddOrReplace<DateTimeOffset, LocalDateTimeOffsetParser>( new LocalDateTimeOffsetParser() );
         AddOrReplace<DateTimeOffset?, NullableLocalDateTimeOffsetParser>( new NullableLocalDateTimeOffsetParser() );
      }

      public void AddOrReplace<TTimestamp, TTimestampParser>( TTimestampParser timestampParser ) where TTimestampParser : ITimestampParser<TTimestamp>
      {
         _timestampParsers.Add( typeof( TTimestamp ), timestampParser );
      }

      public bool Contains<TTimestamp>()
      {
         return _timestampParsers.ContainsKey( typeof( TTimestamp ) );
      }

      public ITimestampParser<TTimestamp> FindTimestampParser<TTimestamp>()
      {
         object obj;
         if( _timestampParsers.TryGetValue( typeof( TTimestamp ), out obj ) && obj is ITimestampParser<TTimestamp> )
         {
            var timestampParser = (ITimestampParser<TTimestamp>)obj;
            return timestampParser;
         }

         throw new InfluxException( string.Format( Errors.CouldNotFindTimestampParser, typeof( TTimestamp ).FullName ) );
      }

      public void Remove<TTimestamp>()
      {
         _timestampParsers.Remove( typeof( TTimestamp ) );
      }
   }
}
