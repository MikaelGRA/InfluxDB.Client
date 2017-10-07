using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// IInfluxRow is an interface that allows using custom types as rows in InfluxDB.
   /// 
   /// When implementing this interface, InfluxAttributes are ignored and the interface 
   /// is used instead.
   /// </summary>
   public interface IInfluxRow : IInfluxRow<DateTime?>
   {
   }

   public interface IInfluxRow<TTimestamp>
   {
      /// <summary>
      /// Sets the timestamp.
      /// </summary>
      /// <param name="value"></param>
      void SetTimestamp( TTimestamp value );

      /// <summary>
      /// Gets the timestamp.
      /// </summary>
      /// <returns></returns>
      TTimestamp GetTimestamp();

      /// <summary>
      /// Sets a field.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      void SetField( string name, object value );

      /// <summary>
      /// Gets a field.
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      object GetField( string name );

      /// <summary>
      /// Sets a tag.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      void SetTag( string name, string value );

      /// <summary>
      /// Gets a tag.
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      string GetTag( string name );

      /// <summary>
      /// Gets all tags contained in the IInfluxRow.
      /// </summary>
      /// <returns></returns>
      IEnumerable<KeyValuePair<string, string>> GetAllTags();

      /// <summary>
      /// Gets all fields cotnained in the IInfluxRow.
      /// </summary>
      /// <returns></returns>
      IEnumerable<KeyValuePair<string, object>> GetAllFields();
   }
}
