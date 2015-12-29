using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Tests
{
   public class InfluxClientFixture : IDisposable
   {
      public const string DatabaseName = "unittestdb";

      public InfluxClient Client { get; set; }

      public InfluxClientFixture()
      {
         Client = new InfluxClient( new Uri( "http://localhost:8086" ), "root", "root" );
         Client.CreateDatabaseIfNotExistsAsync( DatabaseName ).Wait();
      }

      public void Dispose()
      {
         Client.DropDatabaseIfExistsAsync( DatabaseName ).Wait();
      }

      public static readonly string[] Regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
      public static readonly string[] Hosts = new[] { "ma-lt", "surface-book" };

      public static ComputerInfo[] CreateTypedRowsStartingAt( DateTime start, int rows, bool includeNulls )
      {
         var rng = new Random();

         var timestamp = start;
         var infos = new ComputerInfo[ rows ];
         for ( int i = 0 ; i < rows ; i++ )
         {
            long ram = rng.Next( int.MaxValue );
            double cpu = rng.NextDouble();
            string region = Regions[ rng.Next( Regions.Length ) ];
            string host = Hosts[ rng.Next( Hosts.Length ) ];

            if ( includeNulls )
            {
               var info = new ComputerInfo { Timestamp = timestamp, RAM = ram, Region = region };
               infos[ i ] = info;
            }
            else
            {
               var info = new ComputerInfo { Timestamp = timestamp, CPU = cpu, RAM = ram, Host = host, Region = region };
               infos[ i ] = info;
            }

            timestamp = timestamp.AddSeconds( 1 );
         }

         return infos;
      }

      public static DynamicInfluxRow[] CreateDynamicRowsStartingAt( DateTime start, int rows )
      {
         var rng = new Random();

         var timestamp = start;
         var infos = new DynamicInfluxRow[ rows ];
         for ( int i = 0 ; i < rows ; i++ )
         {
            long ram = rng.Next( int.MaxValue );
            double cpu = rng.NextDouble();
            string region = Regions[ rng.Next( Regions.Length ) ];
            string host = Hosts[ rng.Next( Hosts.Length ) ];

            var info = new DynamicInfluxRow();
            info.Fields.Add( "cpu", cpu );
            info.Fields.Add( "ram", ram );
            info.Tags.Add( "host", host );
            info.Tags.Add( "region", region );
            info.Timestamp = timestamp;

            infos[ i ] = info;

            timestamp = timestamp.AddSeconds( 1 );
         }
         return infos;
      }
   }
}
