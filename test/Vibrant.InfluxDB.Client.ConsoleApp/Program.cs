using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.ConsoleApp
{
   class Program
   {
      static void Main( string[] args )
      {
         //var client = new InfluxClient( new Uri( "http://localhost:8086" ) );

         //var from = DateTime.Parse( "21-12-2015 20:39:44" );
         //var to = DateTime.Parse( "23-12-2015 20:28:29" );

         ////var from = new DateTime( 2015, 2, 23, 0, 0, 0, DateTimeKind.Utc );
         ////var to = from.AddSeconds( 250 );

         //var resultSet = client.ReadAsync<DynamicInfluxRow>( $"SELECT * FROM cpu WHERE {from.ToPrecision( TimestampPrecision.Nanosecond )} <= time AND time <= {to.ToPrecision( TimestampPrecision.Nanosecond )}", "mydb" ).Result;

         //Console.WriteLine( resultSet );


         List<object> objs = new List<object> { 23, 54.232, long.MaxValue };

         var str = JsonConvert.SerializeObject( objs );

         var nn = JsonConvert.DeserializeObject<List<object>>( str );

         int d = (int)Convert.ChangeType( 12.42, typeof( double? ) );

         Console.WriteLine(nn);
      }
   }
}
