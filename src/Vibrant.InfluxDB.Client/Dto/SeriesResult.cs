using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   internal class SeriesResult
   {
      [JsonProperty( "tags" )]
      internal Dictionary<string, string> Tags { get; set; }

      [JsonProperty( "name" )]
      internal string Name { get; set; }

      [JsonProperty( "columns" )]
      internal List<string> Columns { get; set; }

      [JsonProperty( "values" )]
      internal List<List<object>> Values { get; set; }
   }
}
