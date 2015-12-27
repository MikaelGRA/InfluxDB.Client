using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vibrant.InfluxDB.Client.Dto
{
   public class ErrorResult
   {
      [JsonProperty( "error" )]
      public string Error { get; set; }
   }
}
