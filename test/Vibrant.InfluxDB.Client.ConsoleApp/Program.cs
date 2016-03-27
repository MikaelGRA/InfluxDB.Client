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

         var query = client.Query<ComputerInfo>( "mydb", "myMeasurement" )
            .Where( x => x.Timestamp < InfluxFunctions.Now() - TimeSpan.FromHours( 1 ) )
            .Where( x => InfluxFunctions.Count( x.RAM ) >= 50 )
            .Select( x => new ProjectedType { test = x.CPU } );

         var items = query.ToList();

         Console.WriteLine( items.Count );
      }
   }

   public class ProjectedType
   {
      public double? test { get; set; }
   }
}
