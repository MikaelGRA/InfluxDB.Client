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
         Settings.Converters.Add( new CustomIntegerConverter() );
         Settings.Culture = CultureInfo.InvariantCulture;
         Settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
      }

      public static string GetParams( object obj )
      {
         return JsonConvert.SerializeObject( obj, Settings );
      }

      /// <summary>
      /// Converter that always adds an 'i' after serialization an integer type.
      /// </summary>
      private class CustomIntegerConverter : JsonConverter
      {
         private static readonly HashSet<Type> Serializable = new HashSet<Type>
         {
            typeof( byte ),
            typeof( sbyte ),
            typeof( short ),
            typeof( ushort ),
            typeof( int ),
            typeof( uint ),
            typeof( long ),
            typeof( ulong ),
            typeof( byte? ),
            typeof( sbyte? ),
            typeof( short? ),
            typeof( ushort? ),
            typeof( int? ),
            typeof( uint? ),
            typeof( long? ),
            typeof( ulong? ),
         };

         public override bool CanConvert( Type objectType )
         {
            return Serializable.Contains( objectType );
         }

         public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
         {
            throw new NotImplementedException();
         }

         public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
         {
            writer.WriteRawValue( Convert.ToString( value, CultureInfo.InvariantCulture ) + "i" );
         }
      }
   }
}
