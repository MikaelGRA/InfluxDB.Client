using System;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Attribute to be placed on properties that are considered tags by InfluxDB.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
   public sealed class InfluxTagAttribute : InfluxAttribute
   {
       /// <summary>
      /// Constructs an InfluxTagAttribute with the given name.
      /// </summary>
      /// <param name="name"></param>
      public InfluxTagAttribute( string name )
      {
         Name = name;
      }

      /// <summary>
      /// Gets the name of the tag used by InfluxDB.
      /// </summary>
      public string Name { get; }
   }
}
