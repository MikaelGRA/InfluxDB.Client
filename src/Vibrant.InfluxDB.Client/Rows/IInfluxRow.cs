using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public interface IInfluxRow
   {
      void SetTimestamp( DateTime? value );

      DateTime? GetTimestamp();

      void SetField( string name, object value );

      object GetField( string name );

      void SetTag( string name, string value );

      string GetTag( string name );

      IEnumerable<KeyValuePair<string, string>> GetAllTags();

      IEnumerable<KeyValuePair<string, object>> GetAllFields();
   }
}
