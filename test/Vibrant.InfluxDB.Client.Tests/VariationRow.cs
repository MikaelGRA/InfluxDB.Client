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

      [InfluxTag( "type= od\\d, name" )]
      public string Type { get; set; }

      [InfluxTag( "categoryTag" )]
      public TestEnum2 CategoryTag { get; set; }

      [InfluxTag( "typeIntTag" )]
      public int IntType { get; set; }

      [InfluxTag( "doubleTag" )]
      public double DoubleTag { get; set; }

      [InfluxField( "m\\es,sa=ge" )]
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

      [InfluxField( "shorty" )]
      public short Shorty { get; set; }

      [InfluxField( "floaty" )]
      public float Floaty { get; set; }

      [InfluxField( "SbytyMcByteFace" )]
      public sbyte SbytyMcByteFace { get; set; }

      [InfluxField( "decimal1" )]
      public decimal Decimal1 { get; set; }

      [InfluxField( "decimal2" )]
      public decimal? Decimal2 { get; set; }

      [InfluxField( "dto1" )]
      public DateTimeOffset Dto1 { get; set; }

      [InfluxField( "dto2" )]
      public DateTimeOffset? Dto2 { get; set; }


      [InfluxField( "decimal3" )]
      public decimal Decimal3 { get; set; }

      [InfluxField( "decimal4" )]
      public decimal? Decimal4 { get; set; }

      [InfluxField( "dto3" )]
      public DateTimeOffset Dto3 { get; set; }

      [InfluxField( "dto4" )]
      public DateTimeOffset? Dto4 { get; set; }




      // override object.Equals
      public override bool Equals( object obj )
      {
         var other = obj as VariationRow;
         if ( other == null )
            return false;

         return Timestamp == other.Timestamp
            && Type == other.Type
            && IntType == other.IntType
            && DoubleTag == other.DoubleTag
            && CategoryTag == other.CategoryTag
            && Message == other.Message
            && Count == other.Count
            && Percent == other.Percent
            && Indicator == other.Indicator
            && OtherTimestamp == other.OtherTimestamp
            && Category == other.Category
            && CategoryNullable == other.CategoryNullable
            && Shorty == other.Shorty
            && Floaty == other.Floaty
            && SbytyMcByteFace == other.SbytyMcByteFace
            && Decimal1 == other.Decimal1
            && Decimal2 == other.Decimal2
            && Dto1 == other.Dto1
            && Dto2 == other.Dto2
            && Decimal3 == other.Decimal3
            && Decimal4 == other.Decimal4
            && Dto3 == other.Dto3
            && Dto4 == other.Dto4;
      }

      // override object.GetHashCode
      public override int GetHashCode()
      {
         // NOT USED
         return 5;
      }
   }
}
