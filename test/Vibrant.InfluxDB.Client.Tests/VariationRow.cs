using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class VariationRow
   {
      [InfluxTimestamp]
      public DateTime Timestamp { get; set; }

      [InfluxTag( "type odd name" )]
      public string Type { get; set; }

      [InfluxTag( "categoryTag" )]
      public TestEnum2 CategoryTag { get; set; }

      [InfluxField( "message" )]
      public string Message { get; set; }

      [InfluxField( "count" )]
      public long Count { get; set; }

      [InfluxField( "percent fun name" )]
      public double Percent { get; set; }

      [InfluxField( "indicator" )]
      public bool Indicator { get; set; }

      [InfluxField( "timestamp" )]
      public DateTime OtherTimestamp { get; set; }

      [InfluxField( "category" )]
      public TestEnum1 Category { get; set; }

      [InfluxField( "categoryField that is nullable" )]
      public TestEnum1? CategoryNullable { get; set; }

      // override object.Equals
      public override bool Equals( object obj )
      {
         var other = obj as VariationRow;
         if ( other == null )
            return false;

         return Timestamp == other.Timestamp
            && Type == other.Type
            && CategoryTag == other.CategoryTag
            && Message == other.Message
            && Count == other.Count
            && Percent == other.Percent
            && Indicator == other.Indicator
            && OtherTimestamp == other.OtherTimestamp
            && Category == other.Category
            && CategoryNullable == other.CategoryNullable;
      }

      // override object.GetHashCode
      public override int GetHashCode()
      {
         // NOT USED
         return 5;
      }
   }
}
