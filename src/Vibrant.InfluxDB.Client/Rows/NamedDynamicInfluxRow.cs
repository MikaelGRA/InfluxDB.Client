using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// A class implementing the IInfluxRow interface that can be used with the DLR. This
   /// allows you to query data that you do not know the structure of.
   /// </summary>
   public class NamedDynamicInfluxRow<TTimestamp> : DynamicObject, IInfluxRow<TTimestamp>, IHaveMeasurementName
   {
      /// <summary>
      /// Constructs a new empty DynamicInfluxRow.
      /// </summary>
      public NamedDynamicInfluxRow()
      {
         Tags = new SortedDictionary<string, string>( StringComparer.Ordinal );
         Fields = new SortedDictionary<string, object>( StringComparer.Ordinal );
      }

      /// <summary>
      /// Gets the timestamp.
      /// </summary>
      /// <returns></returns>
      public TTimestamp GetTimestamp()
      {
         return Timestamp;
      }

      /// <summary>
      /// Sets the timestamp.
      /// </summary>
      /// <param name="value"></param>
      public void SetTimestamp( TTimestamp value )
      {
         Timestamp = value;
      }

      /// <summary>
      /// Gets a field.
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public object GetField( string name )
      {
         return Fields[ name ];
      }

      /// <summary>
      /// Sets a field.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      public void SetField( string name, object value )
      {
         Fields.Add( name, value );
      }

      /// <summary>
      /// Gets a tag.
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public string GetTag( string name )
      {
         return Tags[ name ];
      }

      /// <summary>
      /// Sets a tag.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      public void SetTag( string name, string value )
      {
         Tags.Add( name, value );
      }

      /// <summary>
      /// Gets all tags contained in the IInfluxRow.
      /// </summary>
      /// <returns></returns>
      public IEnumerable<KeyValuePair<string, string>> GetAllTags()
      {
         return Tags;
      }

      /// <summary>
      /// Gets all fields cotnained in the IInfluxRow.
      /// </summary>
      /// <returns></returns>
      public IEnumerable<KeyValuePair<string, object>> GetAllFields()
      {
         return Fields;
      }

      /// <summary>
      /// Gets or sets the timestamp.
      /// </summary>
      public TTimestamp Timestamp { get; set; }

      /// <summary>
      /// Gets or sets the measurement name of this record.
      /// </summary>
      public string MeasurementName { get; set; }

      /// <summary>
      /// Gets a dictionary of all the Tags.
      /// </summary>
      public IDictionary<string, string> Tags { get; private set; }

      /// <summary>
      /// Gets a dictionary of all the Fields.
      /// </summary>
      public IDictionary<string, object> Fields { get; private set; }

      /// <summary>
      /// Gets the member identified by the binder so it can be used
      /// with the DLR.
      /// </summary>
      /// <param name="binder"></param>
      /// <param name="result"></param>
      /// <returns></returns>
      public override bool TryGetMember( GetMemberBinder binder, out object result )
      {
         if( binder.Name == "Timestamp" || binder.Name == InfluxConstants.TimeColumn )
         {
            result = Timestamp;
            return true;
         }

         if( Fields.TryGetValue( binder.Name, out result ) )
         {
            return true;
         }
         string result2 = null;
         if( Tags.TryGetValue( binder.Name, out result2 ) )
         {
            result = result2;
            return true;
         }

         // always return true, simply return a null if a value does not exist
         return true;
      }
   }

   /// <summary>
   /// A class implementing the IInfluxRow interface that can be used with the DLR. This
   /// allows you to query data that you do not know the structure of.
   /// </summary>
   public class NamedDynamicInfluxRow : NamedDynamicInfluxRow<DateTime?>, IInfluxRow
   {
   }
}
