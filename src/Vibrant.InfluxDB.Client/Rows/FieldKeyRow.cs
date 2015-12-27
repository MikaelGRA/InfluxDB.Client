using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public class FieldKeyRow
   {
      [InfluxField( "fieldKey" )]
      public string FieldKey { get; set; }
   }
}
