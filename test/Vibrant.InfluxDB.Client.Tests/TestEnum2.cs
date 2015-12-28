using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public enum TestEnum2
   {
      [EnumMember( Value = "1Value" )]
      Value1,
      [EnumMember( Value = "2ValUE" )]
      Value2,
      [EnumMember( Value = "Va3Lue" )]
      Value3,
   }
}
