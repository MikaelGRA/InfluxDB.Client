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
         var client = new InfluxClient( new Uri( "http://13.95.159.186:8086" ), "root", "root" );

         var query = client.Query<ComputerInfo>( "mydb", "myMeasurementName" )
            //.Where( x => x.Timestamp < InfluxFunctions.Now() - TimeSpan.FromHours( 1 ) )
            .Where( x => x.CPU >= 0.5 )
            .Select( x => new { test = x.CPU } );

         var items = query.ToList();

         Console.WriteLine( items.Count );
      }

      private static ComputerInfo[] CreateTypedRowsStartingAt( DateTime start, int rows )
      {
         var rng = new Random();
         var regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
         var hosts = new[] { "some-host", "some-other-host" };

         var timestamp = start;
         var infos = new ComputerInfo[ rows ];
         for( int i = 0 ; i < rows ; i++ )
         {
            long ram = rng.Next( int.MaxValue );
            double cpu = rng.NextDouble();
            string region = regions[ rng.Next( regions.Length ) ];
            string host = hosts[ rng.Next( hosts.Length ) ];

            var info = new ComputerInfo { Timestamp = timestamp, CPU = cpu, RAM = ram, Host = host, Region = region };
            infos[ i ] = info;

            timestamp = timestamp.AddSeconds( 1 );
         }

         return infos;
      }
   }
}
