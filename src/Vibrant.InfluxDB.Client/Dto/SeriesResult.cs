using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   public class SeriesResult
   {
      [JsonProperty( "tags" )]
      public Dictionary<string, string> Tags { get; set; }

      [JsonProperty( "name" )]
      public string Name { get; set; }

      [JsonProperty( "columns" )]
      public List<string> Columns { get; set; }

      [JsonProperty( "values" )]
      public List<List<object>> Values { get; set; }
   }
}
