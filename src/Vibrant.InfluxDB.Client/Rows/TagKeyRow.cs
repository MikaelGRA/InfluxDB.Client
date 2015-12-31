using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Class representing a row returned by the SHOW TAG KEYS query.
   /// </summary>
   public class TagKeyRow
   {
      /// <summary>
      /// Gets the tag key.
      /// </summary>
      [InfluxField( "tagKey" )]
      public string TagKey { get; private set; }
   }
}
