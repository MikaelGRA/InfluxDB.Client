using System;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Attribute used for specifying an implicit measurement name for a type.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
   public class InfluxMeasurementAttribute : InfluxAttribute
   {
      /// <summary>
      /// Constructs an InfluxMeasurementAttribute with the specified name.
      /// </summary>
      /// <param name="name"></param>
      public InfluxMeasurementAttribute( string name )
      {
         Name = name;
      }

      /// <summary>
      /// Constructs an InfluxMeasurementAttribute without a name. Should be used on a property.
      /// </summary>
      public InfluxMeasurementAttribute()
      {

      }

      /// <summary>
      /// Gets the name of the measurement to use.
      /// </summary>
      public string Name { get; }
   }
}
