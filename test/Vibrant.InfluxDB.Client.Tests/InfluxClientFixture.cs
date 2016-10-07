using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client.Tests
{
   public sealed class InfluxClientFixture : IDisposable
   {
      private bool _disposed;

      public const string DatabaseName = "unittestdb";
      //public static readonly string InfluxHost = "http://winflux.westeurope.cloudapp.azure.com:8086";
      public static readonly string InfluxHost = "http://localhost:8086";
      //public static readonly string InfluxHost = "http://52.174.58.40:8086";

      public InfluxClient Client { get; set; }

      public InfluxClientFixture()
      {
         Client = new InfluxClient( new Uri( InfluxHost ), "root", "root" );
         Client.CreateDatabaseAsync( DatabaseName ).Wait();
      }

      public static readonly string[] Regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
      public static readonly string[] Hosts = new[] { "ma-lt", "surface-book" };
      public static readonly TestEnum1?[] TestEnums = new TestEnum1?[] { null, TestEnum1.Value1, TestEnum1.Value2, TestEnum1.Value3 };

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

      public static EnumeratedRow[] CreateEnumeratedRowsStartingAt( DateTime start, int rows )
      {
         var rng = new Random();

         var timestamp = start;
         var infos = new EnumeratedRow[ rows ];
         for ( int i = 0 ; i < rows ; i++ )
         {
            var value = rng.NextDouble();
            var type = TestEnums[ rng.Next( TestEnums.Length ) ];

            var info = new EnumeratedRow { Timestamp = timestamp, Value = value,  Type = type };
            infos[ i ] = info;

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

      #region IDisposable

      /// <summary>
      /// Destructor.
      /// </summary>
      ~InfluxClientFixture()
      {
         Dispose( false );
      }

      /// <summary>
      /// Disposes the InfluxClient and the internal HttpClient that it uses.
      /// </summary>
      public void Dispose()
      {
         if ( !_disposed )
         {
            Dispose( true );
            _disposed = true;
            GC.SuppressFinalize( this );
         }
      }

      private void Dispose( bool disposing )
      {
         if ( disposing )
         {
            Client.DropDatabaseAsync( DatabaseName ).Wait();
         }
      }

      #endregion
   }
}
