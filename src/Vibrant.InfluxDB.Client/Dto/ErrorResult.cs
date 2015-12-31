using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   internal class ErrorResult
   {
      [JsonProperty( "error" )]
      internal string Error { get; set; }
   }
}
