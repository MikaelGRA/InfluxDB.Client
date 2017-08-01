using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxChunkedResult<TInfluxRow>
   {
      public InfluxChunkedResult( int statementId, string error )
      {
         StatementId = statementId;
         ErrorMessage = error;
         Succeeded = error == null;
      }

      public async Task<InfluxChunkedSeries<TInfluxRow>> GetNextSeriesAsync()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Gets the error message, if the operation did not succeed.
      /// </summary>
      public string ErrorMessage { get; private set; }

      /// <summary>
      /// Gets an indication of whether the operation succeeded.
      /// </summary>
      public bool Succeeded { get; private set; }

      /// <summary>
      /// Gets or sets the statement id.
      /// </summary>
      public int StatementId { get; private set; }
   }
}
