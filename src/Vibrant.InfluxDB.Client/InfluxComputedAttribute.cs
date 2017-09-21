using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Attribute to be placed on properties that are considered fields by InfluxDB.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
   public sealed class InfluxComputedAttribute : InfluxAttribute
   {
      private readonly string _name;

      /// <summary>
      /// Constructs an InfluxFieldAttribute with the given name.
      /// </summary>
      /// <param name="name"></param>
      public InfluxComputedAttribute( string name )
      {
         _name = name;
      }

      /// <summary>
      /// Gets the name of the field used by InfluxDB.
      /// </summary>
      public string Name
      {
         get { return _name; }
      }
   }
}
