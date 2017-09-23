using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   /// <summary>
   /// Interface representing a row returned by the SHOW TAG KEYS query.
   /// </summary>
   /// <typeparam name="TValue"></typeparam>
   public interface ITagValueRow<TValue>
   {
      /// <summary>
      /// Gets the tag key.
      /// </summary>
      string Key { get; }

      /// <summary>
      /// Gets the tag value.
      /// </summary>
      TValue Value { get; }
   }
}
