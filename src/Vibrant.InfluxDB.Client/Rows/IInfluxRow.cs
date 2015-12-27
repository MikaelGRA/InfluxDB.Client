using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public interface IInfluxRow
   {
      void WriteTimestamp( DateTime value );

      DateTime ReadTimestamp();

      void WriteField( string name, object value );

      object ReadField( string name );

      void WriteTag( string name, string value );

      string ReadTag( string name );

      IEnumerable<KeyValuePair<string, string>> GetAllTags();

      IEnumerable<KeyValuePair<string, object>> GetAllFields();
   }
}
