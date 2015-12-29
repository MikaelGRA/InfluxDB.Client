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

            var resultSet = client.ReadAsync<ComputerInfo>( "mydb", $"SELECT * FROM computerInfo" ).Result;

            //var resultSet = client.ShowGrantsAsync( "root" ).Result;

            // resultSet will contain 1 result in the Results collection (or multiple if you execute multiple queries at once)
            var result = resultSet.Results[ 0 ];

            // result will contain 1 series in the Series collection (or potentially multiple if you specify a GROUP BY clause)
            var series = result.Series[ 0 ];

            foreach ( var row in series.Rows )
            {
               Console.WriteLine( "CPU: " + row.CPU );
               Console.WriteLine( "RAM: " + row.RAM );
               // ...
            }

            Console.WriteLine( resultSet );
         }
         catch ( Exception e )
         {

            throw;
         }
      }
   }
}
