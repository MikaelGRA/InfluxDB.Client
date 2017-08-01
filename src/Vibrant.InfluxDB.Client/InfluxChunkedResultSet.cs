using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunkedResultSet<TInfluxRow>
   {
      public InfluxChunkedResultSet()
      {

      }

      public async Task<InfluxChunkedResult<TInfluxRow>> GetNextResultAsync()
      {
         throw new NotImplementedException();
      }
   }
}
