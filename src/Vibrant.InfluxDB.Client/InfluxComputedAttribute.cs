using System;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Attribute to be placed on properties that are considered fields by InfluxDB.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
   public sealed class InfluxComputedAttribute : InfluxAttribute
   {
       /// <summary>
      /// Constructs an InfluxFieldAttribute with the given name.
      /// </summary>
      /// <param name="name"></param>
      public InfluxComputedAttribute( string name )
      {
         Name = name;
      }

      /// <summary>
      /// Gets the name of the field used by InfluxDB.
      /// </summary>
      public string Name { get; }
   }
}
