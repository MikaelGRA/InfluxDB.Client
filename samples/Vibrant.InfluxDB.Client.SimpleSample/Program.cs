using System;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.SimpleSample
{
   public class Program
   {
      static void Main( string[] args ) => MainAsync( args ).GetAwaiter().GetResult();

      static async Task MainAsync( string[] args )
      {
         const string influxHost = "http://ipv4.fiddler:8086"; // "http://localhost:8086";
         const string databaseName = "mydb";

         var client = new InfluxClient( new Uri( influxHost ) );

         await client.CreateDatabaseAsync( databaseName );

         await Should_Write_Typed_Rows_To_Database( databaseName, client );
         await Should_Query_And_Display_Typed_Data( databaseName, client );
         await Should_Query_With_Parameters_And_Display_Typed_Data( databaseName, client );

         await client.DropDatabaseAsync( databaseName );

         Console.WriteLine( "Press any key to exit..." );
         Console.ReadKey();
      }

      public static async Task Should_Write_Typed_Rows_To_Database( string db, InfluxClient client )
      {
         var infos = CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 500 );
         await client.WriteAsync( db, "myMeasurementName", infos );
      }

      public static async Task Should_Query_And_Display_Typed_Data( string db, InfluxClient client )
      {
         var resultSet = await client.ReadAsync<ComputerInfo>( db, "SELECT * FROM myMeasurementName" );

         // resultSet will contain 1 result in the Results collection (or multiple if you execute multiple queries at once)
         var result = resultSet.Results[ 0 ];

         // result will contain 1 series in the Series collection (or potentially multiple if you specify a GROUP BY clause)
         var series = result.Series[ 0 ];

         Console.WriteLine( $"{"Timestamp",10}{"Region",15}{"Host",20}{"CPU",20}{"RAM",15}" );

         // series.Rows will be the list of ComputerInfo that you queried for
         foreach( var row in series.Rows )
         {
            Console.WriteLine( $"{row.Timestamp,10}{row.Region,15}{row.Host,20}{row.CPU,20}{row.RAM,15}" );
         }
      }

      public static async Task Should_Query_With_Parameters_And_Display_Typed_Data( string db, InfluxClient client )
      {
         var resultSet = await client.ReadAsync<ComputerInfo>( 
            db,
            "SELECT * FROM myMeasurementName WHERE time >= $myParam", 
            new { myParam = new DateTime( 2010, 1, 1, 1, 1, 3, DateTimeKind.Utc ) } );

         // resultSet will contain 1 result in the Results collection (or multiple if you execute multiple queries at once)
         var result = resultSet.Results[ 0 ];

         // result will contain 1 series in the Series collection (or potentially multiple if you specify a GROUP BY clause)
         var series = result.Series[ 0 ];

         Console.WriteLine( $"{"Timestamp",10}{"Region",15}{"Host",20}{"CPU",20}{"RAM",15}" );

         // series.Rows will be the list of ComputerInfo that you queried for
         foreach( var row in series.Rows )
         {
            Console.WriteLine( $"{row.Timestamp,10}{row.Region,15}{row.Host,20}{row.CPU,20}{row.RAM,15}" );
         }
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
