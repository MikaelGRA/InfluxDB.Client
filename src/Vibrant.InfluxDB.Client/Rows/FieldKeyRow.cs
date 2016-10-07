using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW FIELD KEYS query.
   /// </summary>
   public class FieldKeyRow
   {
      /// <summary>
      /// Gets the field key.
      /// </summary>
      [InfluxField( "fieldKey" )]
      public string FieldKey { get; private set; }

      /// <summary>
      /// Gets the field type.
      /// </summary>
      [InfluxField( "fieldType" )]
      public string FieldType { get; private set; }
   }
}
