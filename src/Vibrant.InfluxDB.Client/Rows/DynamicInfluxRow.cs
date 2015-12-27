using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class DynamicInfluxRow : DynamicObject, IInfluxRow
   {
      public DateTime ReadTimestamp()
      {
         return Timestamp;
      }

      public void WriteTimestamp( DateTime value )
      {
         Timestamp = value;
      }

      public object ReadField( string name )
      {
         return Fields[ name ];
      }

      public void WriteField( string name, object value )
      {
         Fields.Add( name, value );
      }

      public string ReadTag( string name )
      {
         return Tags[ name ];
      }

      public void WriteTag( string name, string value )
      {
         Tags.Add( name, value );
      }

      public IEnumerable<KeyValuePair<string, string>> GetAllTags()
      {
         return Tags;
      }

      public IEnumerable<KeyValuePair<string, object>> GetAllFields()
      {
         return Fields;
      }

      public DateTime Timestamp { get; set; }

      public IDictionary<string, string> Tags { get; private set; }

      public IDictionary<string, object> Fields { get; private set; }

      public DynamicInfluxRow()
      {
         Tags = new Dictionary<string, string>();
         Fields = new Dictionary<string, object>();
      }

      public override bool TryGetMember( GetMemberBinder binder, out object result )
      {
         if ( binder.Name == "Timestamp" || binder.Name == InfluxConstants.TimeColumn )
         {
            // TODO: What if null???
            result = Timestamp;
            return true;
         }

         if ( Fields.TryGetValue( binder.Name, out result ) )
         {
            return true;
         }
         string result2 = null;
         if ( Tags.TryGetValue( binder.Name, out result2 ) )
         {
            result = result2;
            return true;
         }
         return false;
      }
   }
}
