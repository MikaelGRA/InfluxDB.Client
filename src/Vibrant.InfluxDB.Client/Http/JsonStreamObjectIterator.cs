using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Http
{
   internal class JsonStreamObjectIterator
   {
      private JsonSerializer _serializer;
      private JsonTextReader _reader;

      public JsonStreamObjectIterator( Stream stream, JsonSerializer serializer )
      {
         _serializer = serializer;
         _reader = new JsonTextReader( new StreamReader( stream, Encoding.UTF8 ) );
         _reader.SupportMultipleContent = true;
      }

      public T ReadNext<T>(  )
      {
         if( !_reader.Read() )
         {
            return default( T );
         }

         T result = _serializer.Deserialize<T>( _reader );
         return result;
      }
   }
}
