using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
    public class QueryResult
    {
      [JsonProperty( "results" )]
      public List<SeriesResultWrapper> Results { get; set; }
   }
}
