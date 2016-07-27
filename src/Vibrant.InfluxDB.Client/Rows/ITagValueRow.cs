using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Rows
{
   public interface ITagValueRow<TValue>
   {
      string Key { get; }

      TValue Value { get; }
   }
}
