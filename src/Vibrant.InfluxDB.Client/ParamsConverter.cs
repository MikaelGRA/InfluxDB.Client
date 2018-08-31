using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vibrant.InfluxDB.Client
{
   internal static class ParamsConverter
   {
      private static readonly JsonSerializerSettings Settings;

      static ParamsConverter()
      {
         Settings = new JsonSerializerSettings();
         Settings.Formatting = Formatting.None;
         Settings.Converters.Add( new StringEnumConverter() );
         Settings.Culture = CultureInfo.InvariantCulture;
         Settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
      }

      public static string GetParams( object obj )
      {
         return JsonConvert.SerializeObject( obj, Settings );
      }
   }
}
