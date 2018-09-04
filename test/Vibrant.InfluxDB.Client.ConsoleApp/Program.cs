using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.ConsoleApp
{
   public class Program
   {
      private const string TestDatabase = "test-app";
      private const string MeasurementName = "schema.computerInfo1";
      private static readonly string[] TagNames = { "Host", "Region" };
      private static readonly string[] Hosts = { "some-host", "some-other-host" };
      private static readonly string[] Regions = { "west-eu", "north-eu", "west-us", "east-us", "asia" };

      public static async Task Main(string[] args)
      {
         try
         {
            var client = new InfluxClient(new Uri("http://localhost:8086"));
            var database = await client.ShowDatabasesAsync();

            if ((await client.ShowDatabasesAsync()).Series.Single(s => s.Name == "databases").Rows.Any(d => d.Name == $"{TestDatabase}"))
            {
               await client.DropDatabaseAsync(TestDatabase);
            }
            await client.CreateDatabaseAsync(TestDatabase);

            var from = DateTime.UtcNow;
            var infos = CreateTypedRowsStartingAt(from, 1);

            //await client.WriteAsync(TestDatabase, MeasurementName, infos, new InfluxWriteOptions
            //{
            //    DefaultTags =
            //    {
            //        { TagNames[0], Hosts[0] },
            //        { TagNames[1], Regions[0] }
            //    }
            //});

            await client.WriteAsync(TestDatabase, infos, new InfluxWriteOptions
            {
               DefaultTags =
                    {
                        { TagNames[0], Hosts[0] },
                        { TagNames[1], Regions[0] }
                    }
            });

            var records = await client.ReadAsync<ComputerInfoResult>(TestDatabase, $"select * from \"{MeasurementName}\"");
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }

         Console.WriteLine("Done.");
         Console.ReadKey();
      }

      private static ComputerInfo[] CreateTypedRowsStartingAt(DateTime start, int rows)
      {
         var rng = new Random();

         var timestamp = start;
         var infos = new ComputerInfo[rows];
         for (int i = 0; i < rows; i++)
         {
            long ram = rng.Next(int.MaxValue);
            double cpu = rng.NextDouble();
            //string region = Regions[rng.Next(Regions.Length)];
            //string host = Hosts[rng.Next(Hosts.Length)];

            //var info = new ComputerInfo { Timestamp = timestamp, CPU = cpu, RAM = ram, Host = host, Region = region };
            var info = new ComputerInfo { Timestamp = timestamp, CPU = cpu, RAM = ram };
            infos[i] = info;

            timestamp = timestamp.AddSeconds(1);
         }

         return infos;
      }
   }
}
