using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
         try
         {
            var client = new InfluxClient( new Uri( "http://localhost:8086" ), "root", "root" );

            //var from = DateTime.Parse( "21-12-2015 20:39:44" );
            //var to = DateTime.Parse( "23-12-2015 20:28:29" );

            //var resultSet = client.ReadAsync<DynamicInfluxRow>( $"SELECT * FROM cpu WHERE {from.ToPrecision( TimestampPrecision.Nanosecond )} <= time AND time <= {to.ToPrecision( TimestampPrecision.Nanosecond )}", "mydb" ).Result;

            var resultSet = client.ShowGrantsAsync( "root" ).Result;

            Console.WriteLine( resultSet );
         }
         catch ( Exception e )
         {

            throw;
         }
      }
   }
}
