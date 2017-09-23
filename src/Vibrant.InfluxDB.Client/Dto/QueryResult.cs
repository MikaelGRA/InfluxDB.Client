using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   internal class QueryResult
   {
      [JsonProperty( "results" )]
      internal List<SeriesResultWrapper> Results { get; set; }
   }
}
