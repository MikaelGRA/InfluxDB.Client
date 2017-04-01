using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   internal class SeriesResultWrapper
    {
      [JsonProperty( "series" )]
      internal List<SeriesResult> Series { get; set; }

      [JsonProperty( "error" )]
      internal string Error { get; set; }

      [JsonProperty( "statement_id" )]
      internal int StatementId { get; set; }
   }
}
