using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW TAG KEYS query.
   /// </summary>
   public class TagValueRow : ITagValueRow<string>
   {
      /// <summary>
      /// Gets the tag key.
      /// </summary>
      [InfluxField( "key" )]
      public string Key { get; private set; }

      /// <summary>
      /// Gets the tag value.
      /// </summary>
      [InfluxField( "value" )]
      public string Value { get; private set; }
   }
}
