using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.ConsoleApp
{
   public class Program
   {
      public static void Main( string[] args )
      {
         var client = new InfluxClient( new Uri( "http://40.113.119.250:8086" ), "root", "root" );

         var query = client.Query<ComputerInfo>( "mydb", "myMeasurement" )
            .Where( x => x.CPU >= 40.0 )
            .Where( x => x.RAM == 5000 );

         var iql = query.ToString();
      }
   }
}
