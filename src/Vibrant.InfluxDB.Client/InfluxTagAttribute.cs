using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// Attribute to be placed on properties that are considered tags by InfluxDB.
   /// </summary>
   [AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
   public sealed class InfluxTagAttribute : InfluxAttribute
   {
      private readonly string _name;

      /// <summary>
      /// Constructs an InfluxTagAttribute with the given name.
      /// </summary>
      /// <param name="name"></param>
      public InfluxTagAttribute( string name )
      {
         _name = name;
      }

      public string Name
      {
         get { return _name; }
      }
   }
}
