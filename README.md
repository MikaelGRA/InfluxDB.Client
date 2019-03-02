# InfluxDB Client for .NET
<!-- [![Build status](https://ci.appveyor.com/api/projects/status/0rkna8pevwiv9acy/branch/master?svg=true)](https://ci.appveyor.com/project/MikaelGRA/influxdb-client/branch/master) -->

This library makes it easy to be a client for InfluxDB on .NET!

The basic idea behind the library is that it should be able to turn queries directly into objects of your own classes. Much like micro-ORMS such as dapper.

The goal is that we want to be able to support LINQ syntax in the future.

## Installation

Install it through nuget with the following command.

```
Install-Package Vibrant.InfluxDB.Client
```

The package can be found [here](https://www.nuget.org/packages/Vibrant.InfluxDB.Client/).

Or you can simply grab it in one of the github releases.

## Reading/Writing

The library exposes all HTTP operations on InfluxDB (1.0+) and can be used for reading/writing data to/from in two primary ways:
 * Using your own POCO classes.
 * Using dynamic classes.
 
A simple example of how to use the library is available [here](https://github.com/MikaelGRA/InfluxDB.Client/blob/master/samples/Vibrant.InfluxDB.Client.SimpleSample/Program.cs).

### Using your own POCO classes

Start by defining a class that represents a row in InfluxDB that you want to store.

```c#
public class ComputerInfo
{
   [InfluxTimestamp]
   public DateTime Timestamp { get; set; }

   [InfluxTag( "host" )]
   public string Host { get; set; }

   [InfluxTag( "region" )]
   public string Region { get; set; }

   [InfluxField( "cpu" )]
   public double CPU { get; set; }

   [InfluxField( "ram" )]
   public long RAM { get; set; }
}
```

On your POCO class you must specify these things:
 * 1 property with the type DateTime, DateTime?, DateTimeOffset or DateTimeOffset? as the timestamp used in InfluxDB by adding the [InfluxTimestamp] attribute.
 * 0-* properties with the type string, long, ulong, int, uint, short, ushort, byte, sbyte, double, float, bool, DateTime, DateTimeOffset, decimal or a user-defined enum (nullables too) with the [InfluxTag] attribute that InfluxDB will use as indexed tags. Note that all tags in InfluxDB is still stored a string. The library will simply making the conversion to the specified type automatically.
 * 1-* properties with the type string, long, ulong, int, uint, short, ushort, byte, sbyte, double, float, bool, DateTime, DateTimeOffset, decimal or a user-defined enum (nullables too) with the [InfluxField] attribute that InfluxDB will use as fields.

Once you've defined your class, you're ready to use the InfluxClient, which is the main entry point to the API:

Here's how to write to the database:

```c#
private ComputerInfo[] CreateTypedRowsStartingAt( DateTime start, int rows )
{
   var rng = new Random();
   var regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
   var hosts = new[] { "some-host", "some-other-host" };

   var timestamp = start;
   var infos = new ComputerInfo[ rows ];
   for ( int i = 0 ; i < rows ; i++ )
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

public async Task Should_Write_Typed_Rows_To_Database()
{
   var client = new InfluxClient( new Uri( "http://localhost:8086" ) );
   var infos = CreateTypedRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 500 );
   await client.WriteAsync( "mydb", "myMeasurementName", infos );
}
```

Here's how to query from the database:
```c#
public async Task Should_Query_Typed_Data()
{
   var resultSet = await client.ReadAsync<ComputerInfo>( "mydb", "SELECT * FROM myMeasurementName" );
   
   // resultSet will contain 1 result in the Results collection (or multiple if you execute multiple queries at once)
   var result = resultSet.Results[ 0 ];
   
   // result will contain 1 series in the Series collection (or potentially multiple if you specify a GROUP BY clause)
   var series = result.Series[ 0 ];
   
   // series.Rows will be the list of ComputerInfo that you queried for
   foreach ( var row in series.Rows )
   {
      Console.WriteLine( "Timestamp: " + row.Timestamp );
      Console.WriteLine( "CPU: " + row.CPU );
      Console.WriteLine( "RAM: " + row.RAM );
      // ...
   }
}
```

### Using dynamic classes

POCO classes does not fit every use-case. This becomes obvious once you are implementing a system and you don't know what the fields/tags will be at compile time. In this case you must use dynamic classes.

In order for this to work, you must use the interface IInfluxRow that specifies reading/writing methods for tags and fields. This library already includes one implementatioon of this interfaces that uses dictionaries and has basic support for the DLR. This class is called DynamicInfluxRow. 

Here's how to write using dynamic classes.

```c#
private DynamicInfluxRow[] CreateDynamicRowsStartingAt( DateTime start, int rows )
{
   var rng = new Random();
   var regions = new[] { "west-eu", "north-eu", "west-us", "east-us", "asia" };
   var hosts = new[] { "some-host", "some-other-host" };
   
   var timestamp = start;
   var infos = new DynamicInfluxRow[ rows ];
   for ( int i = 0 ; i < rows ; i++ )
   {
      long ram = rng.Next( int.MaxValue );
      double cpu = rng.NextDouble();
      string region = regions[ rng.Next( regions.Length ) ];
      string host = hosts[ rng.Next( hosts.Length ) ];

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

public async Task Should_Write_Dynamic_Rows_To_Database()
{
   var client = new InfluxClient( new Uri( "http://localhost:8086" ) );
   var infos = CreateDynamicRowsStartingAt( new DateTime( 2010, 1, 1, 1, 1, 1, DateTimeKind.Utc ), 500 );
   await client.WriteAsync( "mydb", "myMeasurementName", infos );
}
```

Do note, that if you use dynamic classes, user-defined enums and DateTimes as fields/tags are not supported, as there is no way to differentiate between a string and an enum/DateTime.

Also note, if you want to use custom timestamp type or DateTimeOffset with this interface, you can use the generic IInfluxRow<TTimestamp> interface or DynamicInfluxRow<TTimestamp> class.

Here's how to query from the database:

```c#
public async Task Should_Query_Dynamic_Data()
{
   var resultSet = await client.ReadAsync<DynamicInfluxRow>( "mydb", "SELECT * FROM myMeasurementName" );
   
   // resultSet will contain 1 result in the Results collection (or multiple if you execute multiple queries at once)
   var result = resultSet.Results[ 0 ];
   
   // result will contain 1 series in the Series collection (or potentially multiple if you specify a GROUP BY clause)
   var series = result.Series[ 0 ];
   
   // series.Rows will be the list of DynamicInfluxRow that you queried for (which can be cast to dynamic)
   foreach ( dynamic row in series.Rows )
   {
      Console.WriteLine( "Timestamp: " + row.time ); // Can also access row.Timestamp
      Console.WriteLine( "CPU: " + row.cpu );
      Console.WriteLine( "RAM: " + row.ram );
      // ...
   }
}
```

## Read only operations

Often, you may not be selecting the exact structure that you are also inserting. Maybe you are doing some aggregation or calculations on the data that you are retrieving that changes the name of the returned columns.

In this case, you can simply define a new class and use the InfluxComputedAttribute. Any columns that matches the name specified in the attribute (tag or field, aggregated or not) will go into the property with this attribute.

```c#
[InfluxComputedAttribute]
```

Classes with this attribute should not be used for insertion, as there is no way for the client to know if it is a field or tag.

If you are using the IInfluxRow interface (DynamicInfluxRow, for instance), then the "Fields" collection is filled up with all columns that does not match a known tag for the specific measurement.

## Chunking

Sometimes you may retrieve a massive amount of data from the database, so much in fact, that keeping it all in memory at any one time is unfeasible. In this case you need the chunking feature provided by InfluxDB. You can take advantage of this feature by enabling chunking through the InfluxQueryOptions class. When enabled the client will provide the chunking options to InfluxDB when it is retrieving data.

The default ReadAsync operations will, however, simply read all chunks before returning control to the user. To support scenarios where you want to read the data chunk by chunk, you can instead use the method ReadChunkedAsync. This will return a different type of result set that allows you to asynchonously iterate over all the chunks (while still maintaining the structure of the queries that you initially made). Here's an example taken from the unit tests:

```C#
[Fact]
public async Task Should_Write_And_Query_Deferred_Grouped_Data_With_Multi_Query()
{
   var start = new DateTime( 2011, 1, 1, 1, 1, 1, DateTimeKind.Utc );
   var infos = InfluxClientFixture.CreateTypedRowsStartingAt( start, 5000, false );
   await client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo4", infos );
   await client.WriteAsync( InfluxClientFixture.DatabaseName, "groupedComputerInfo5", infos );

   var chunkedResultSet = await client.ReadChunkedAsync<ComputerInfo>( 
      InfluxClientFixture.DatabaseName, 
      $"SELECT * FROM groupedComputerInfo4 GROUP BY region;SELECT * FROM groupedComputerInfo5 GROUP BY region", 
      new InfluxQueryOptions { ChunkSize = 200 } );

   InfluxChunkedResult<ComputerInfo> currentResult;
   InfluxChunkedSeries<ComputerInfo> currentSerie;
   InfluxChunk<ComputerInfo> currentChunk;
   int resultCount = 0;
   int serieCount = 0;
   int rowCount = 0;

   using( chunkedResultSet )
   {
      while( ( currentResult = await chunkedResultSet.GetNextResultAsync() ) != null )
      {
         while( ( currentSerie = await currentResult.GetNextSeriesAsync() ) != null )
         {
            while( ( currentChunk = await currentSerie.GetNextChunkAsync() ) != null )
            {
               rowCount += currentChunk.Rows.Count;
            }
            serieCount++;
         }
         resultCount++;
      }
   }

   Assert.Equal( 1 * 2, resultCount );
   Assert.Equal( InfluxClientFixture.Regions.Length * 2, serieCount );
   Assert.Equal( 5000 * 2, rowCount );
}
```

In the coming versions of C# there will be the capability to iterate over async enumerables, and once this feature hits, I will support that as well. See the video below:

https://channel9.msdn.com/Blogs/Seth-Juarez/A-Preview-of-C-8-with-Mads-Torgersen#time=16m30s

## Preserving timezone offsets

When specifying the timezone clause, influxdb will return timestamps with their offsets. You can preserve this offset in the returned timestamp by using either a DateTimeOffset or Nullable<DateTimeOffset> as the timestamp type. If you use DateTime or Nullable<DateTIme> as the timestamp type, the timestamp will always be converted to UTC.
   
### Custom timestamp type

Alternatively, you can implement your own ITimestampParser to support custom types, for instance NodaTime. Once implemented you can register on the InfluxClient. Simply implement the following interface:

```C#
/// <summary>
/// ITimestampParser is responsible for parsing the 'time' column
/// of data returned, allowing use of custom DateTime types.
/// </summary>
/// <typeparam name="TTimestamp"></typeparam>
public interface ITimestampParser<TTimestamp>
{
   /// <summary>
   /// Parses a epoch time (UTC) or ISO8601-timestamp (potentially with offset) to a date and time.
   /// This is used when reading data from influxdb.
   /// </summary>
   /// <param name="precision">TimestampPrecision provided by the current InfluxQueryOptions.</param>
   /// <param name="epochTimeLongOrIsoTimestampString">The raw value returned by the query.</param>
   /// <returns>The parsed timestamp.</returns>
   TTimestamp ToTimestamp( TimestampPrecision? precision, object epochTimeLongOrIsoTimestampString );

   /// <summary>
   /// Converts the timestamp to epoch time (UTC). This is used when writing data to influxdb.
   /// </summary>
   /// <param name="precision">TimestampPrecision provided by the current InfluxWriteOptions.</param>
   /// <param name="timestamp">The timestamp to convert.</param>
   /// <returns>The UTC epoch time.</returns>
   long ToEpoch( TimestampPrecision precision, TTimestamp timestamp );
}
```

## Writing to different measurement names in single call

Often you may want to write to multiple measurements with different measurement names by executing a single call. 

This can be achieved by either implementing the following interface on your POCO classes:

```C#

   /// <summary>
   /// Interface that can be used to specify a per-row measurement name.
   /// </summary>
   public interface IHaveMeasurementName
   {
      /// <summary>
      /// Gets or sets the measurement name.
      /// </summary>
      string MeasurementName { get; set; }
   }
   
```

Or by putting the [InfluxMeasurement] attribute on your class or property definitions of your POCO class.

When using it on a class you must specify a name, like so:

```C#

   [InfluxMeasurement( "MyTableName" )]
   public class ClassWithMeasurementName
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxField( "cpu" )]
      internal double? CPU { get; set; }
   }

```

When using it on a property, don't specify a name. It will use the property value (which must be a string):

```C#

   public class ClassWithMeasurementName
   {
      [InfluxTimestamp]
      internal DateTime Timestamp { get; set; }

      [InfluxField( "cpu" )]
      internal double? CPU { get; set; }

      [InfluxMeasurement]
      public string TableName { get; set; }
   }

```

When using one of these approaches you can then use the following overloads WriteAsync method, which don't take a measurementName as an argument:

```C#

      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows )
      
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
      
```

In case you want to do this with dynamic classes, you can simply use the NamedDynamicInfluxRow (which implements the IHaveMeasurementName interface).

The following priority is used when determine which measurement to write a record to in case multiple approaches are used:
 * The name provided in the WriteAsync method.
 * The name provided by the IHaveMeasurementName interface
 * The name provided by the property annotated with the InfluxMeasurementAttribute.
 * The name provided in the class annotated with the InfluxMeasurementAttribute.
 
In case you use the IHaveMeasurementName or a property with the InfluxMeasurementAttribute, the measurement name will be written to that property during read operations.

## Parameter binding and SQL injection prevention

The InfluxClient also supports parameter binding to support the prevention of sql injection.

To use this, simply use the methods that takes the parameter "object parameters". This can be an anonymous object, dictionary or any object that supports JSON serialization through Newtonsoft.Json.

When parameterizing values in the object or dictionary, do not prefix the names with $, like the names are in the actual query.

Here's an example of what that might look like:

```C#
var resultSet = await client.ReadAsync<ComputerInfo>( 
   db,
   "SELECT * FROM myMeasurementName WHERE time >= $myParam", 
   new { myParam = new DateTime( 2010, 1, 1, 1, 1, 3, DateTimeKind.Utc ) } );
```

Any type that you would usually use through this client library can be used as a parameter.

## Other operations

The main interface for interacting with influxdb can be seen below.

```c#
public interface IInfluxClient
{
   /// <summary>
   /// Gets the default query optionns.
   /// </summary>
   InfluxQueryOptions DefaultQueryOptions { get; }
   /// <summary>
   /// Gets the default write options.
   /// </summary>
   InfluxWriteOptions DefaultWriteOptions { get; }
   /// <summary>
   /// Gets or sets the timeout for all requests made.
   /// </summary>
   TimeSpan Timeout { get; set; }
   /// <summary>
   /// Gets the timestamp parser registry.
   /// </summary>
   ITimestampParserRegistry TimestampParserRegistry { get; }
      
   /// <summary>
   /// Executes an arbitrary command that does not return a table.
   /// </summary>
   /// <param name="commandOrQuery"></param>
   /// <param name="db"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db, object parameters );
   /// <summary>
   /// Executes an arbitrary command that returns a table as a result.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="commandOrQuery"></param>
   /// <param name="db"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db, object parameters ) where TInfluxRow : new();

   /// <summary>
   /// Executes a ping and waits for the leader to respond.
   /// </summary>
   /// <param name="secondsToWaitForLeader"></param>
   /// <returns></returns>
   Task<InfluxPingResult> PingAsync( int? secondsToWaitForLeader );

   /// <summary>
   /// Executes the query and returns the result with the specified query options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="query"></param>
   /// <param name="db"></param>
   /// <param name="options"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options ) where TInfluxRow : new();
   /// <summary>
   /// Executes the query and returns a deferred result that can be iterated over as they
   /// are returned by the database.
   /// 
   /// It does not make sense to use this method unless you are returning a big payload and
   /// have enabled chunking through InfluxQueryOptions.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="db"></param>
   /// <param name="query"></param>
   /// <param name="options"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options ) where TInfluxRow : new();
   /// <summary>
   /// Writes the rows with the specified write options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="rows"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options ) where TInfluxRow : new();
   /// <summary>
   /// Deletes data in accordance with the specified query
   /// </summary>
   /// <param name="db"></param>
   /// <param name="deleteQuery"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   Task<InfluxResult> DeleteAsync( string db, string deleteQuery, object parameters );
}
```

In addition to this interface, a lot of extension methods that builds on top of it is provided:

```c#
public static class InfluxClientExtensions
{
   #region Raw Operations

   /// <summary>
   /// Executes an arbitrary command that returns a table as a result.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, string db );

   /// <summary>
   /// Executes an arbitrary command or query that returns a table as a result.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery );

   /// <summary>
   /// Executes an arbitrary command or query that returns a table as a result.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, object parameters );

   /// <summary>
   /// Executes an arbitrary command that does not return a table.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, string db );

   /// <summary>
   /// Executes an arbitrary command that does not return a table.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery );

   /// <summary>
   /// Executes an arbitrary command that does not return a table.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="commandOrQuery"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, object parameters );

   #endregion

   #region System Monitoring

   /// <summary>
   /// Shows InfluxDB stats.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>( this IInfluxClient client );

   /// <summary>
   /// Shows InfluxDB diagnostics.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>( this IInfluxClient client );

   /// <summary>
   /// Shows Shards.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static async Task<InfluxResult<ShardRow>> ShowShards( this IInfluxClient client );

   #endregion

   #region Authentication and Authorization

   /// <summary>
   /// CREATE a new admin user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <param name="password"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet> CreateAdminUserAsync( this IInfluxClient client, string username, string password );

   /// <summary>
   /// CREATE a new non-admin user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <param name="password"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> CreateUserAsync( this IInfluxClient client, string username, string password );

   /// <summary>
   /// GRANT administrative privileges to an existing user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> GrantAdminPrivilegesAsync( this IInfluxClient client, string username );

   /// <summary>
   /// GRANT READ, WRITE or ALL database privileges to an existing user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="privilege"></param>
   /// <param name="db"></param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> GrantPrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username );

   /// <summary>
   /// REVOKE administrative privileges from an admin user
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> RevokeAdminPrivilegesAsync( this IInfluxClient client, string username );

   /// <summary>
   /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="privilege"></param>
   /// <param name="db"></param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> RevokePrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username );

   /// <summary>
   /// SET a user’s password.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <param name="password"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> SetPasswordAsync( this IInfluxClient client, string username, string password );

   /// <summary>
   /// DROP a user.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropUserAsync( this IInfluxClient client, string username );

   /// <summary>
   /// SHOW all existing users and their admin status.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static async Task<InfluxResult<UserRow>> ShowUsersAsync( this IInfluxClient client );

   /// <summary>
   /// SHOW a user’s database privileges.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="username"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( this IInfluxClient client, string username );

   #endregion

   #region Database Management

   /// <summary>
   /// Create a database with CREATE DATABASE.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> CreateDatabaseAsync( this IInfluxClient client, string db );

   /// <summary>
   /// Delete a database with DROP DATABASE
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropDatabaseAsync( this IInfluxClient client, string db );

   /// <summary>
   /// Delete series with DROP SERIES
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName );

   /// <summary>
   /// Delete series with DROP SERIES
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="where"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName, string where );

   /// <summary>
   /// Delete measurements with DROP MEASUREMENT
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="measurementName"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropMeasurementAsync( this IInfluxClient client, string db, string measurementName );

   /// <summary>
   /// Create retention policies with CREATE RETENTION POLICY
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="policyName"></param>
   /// <param name="db"></param>
   /// <param name="duration"></param>
   /// <param name="replicationLevel"></param>
   /// <param name="isDefault"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault );

   /// <summary>
   /// Create retention policies with CREATE RETENTION POLICY
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="policyName"></param>
   /// <param name="db"></param>
   /// <param name="duration"></param>
   /// <param name="replicationLevel"></param>
   /// <param name="shardGroupDuration"></param>
   /// <param name="isDefault"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault );

   /// <summary>
   /// Modify retention policies with ALTER RETENTION POLICY
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="policyName"></param>
   /// <param name="db"></param>
   /// <param name="duration"></param>
   /// <param name="replicationLevel"></param>
   /// <param name="isDefault"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault );

   /// <summary>
   /// Modify retention policies with ALTER RETENTION POLICY
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="policyName"></param>
   /// <param name="db"></param>
   /// <param name="duration"></param>
   /// <param name="replicationLevel"></param>
   /// <param name="shardGroupDuration"></param>
   /// <param name="isDefault"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault );

   /// <summary>
   /// Delete retention policies with DROP RETENTION POLICY
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="policyName"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropRetentionPolicyAsync( this IInfluxClient client, string db, string policyName );

   #endregion

   #region Continous Queries

   /// <summary>
   /// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( this IInfluxClient client, string db );

   /// <summary>
   /// Creates a continuous query.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="name"></param>
   /// <param name="db"></param>
   /// <param name="continuousQuery"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> CreateContinuousQuery( this IInfluxClient client, string db, string name, string continuousQuery );

   /// <summary>
   /// Drops a continuous query.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="name"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult> DropContinuousQuery( this IInfluxClient client, string db, string name );

   #endregion

   #region Schema Exploration

   /// <summary>
   /// Get a list of all the databases in your system.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync( this IInfluxClient client );

   /// <summary>
   /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( this IInfluxClient client, string db );

   /// <summary>
   /// The SHOW SERIES query returns the distinct series in your database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db );

   /// <summary>
   /// The SHOW SERIES query returns the distinct series in your database.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName );

   /// <summary>
   /// The SHOW SERIES query returns the distinct series in your database.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="where"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName, string where );

   /// <summary>
   /// The SHOW MEASUREMENTS query returns the measurements in your database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db );

   /// <summary>
   /// The SHOW MEASUREMENTS query returns the measurements in your database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="where"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db, string where );

   /// <summary>
   /// The SHOW MEASUREMENTS query returns the measurements in your database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementRegex"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex );

   /// <summary>
   /// The SHOW MEASUREMENTS query returns the measurements in your database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementRegex"></param>
   /// <param name="where"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex, string where );

   /// <summary>
   /// SHOW TAG KEYS returns the tag keys associated with each measurement.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db );

   /// <summary>
   /// SHOW TAG KEYS returns the tag keys associated with each measurement.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db, string measurementName );

   /// <summary>
   /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="tagKey"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey );

   /// <summary>
   /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <typeparam name="TValue"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="tagKey"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey, string measurementName );

   /// <summary>
   /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="tagKey"></param>
   /// <returns></returns>
   public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey );

   /// <summary>
   /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="tagKey"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey, string measurementName );


   /// <summary>
   /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db );

   /// <summary>
   /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <returns></returns>
   public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db, string measurementName );

   #endregion

   #region Ping

   /// <summary>
   /// Executes a ping.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <returns></returns>
   public static Task<InfluxPingResult> PingAsync( this IInfluxClient client );

   #endregion

   #region Data Management

   /// <summary>
   /// Writes the rows with default write options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="rows"></param>
   /// <returns></returns>
   public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, string measurementName, IEnumerable<TInfluxRow> rows );

   /// <summary>
   /// Writes the rows with default write options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="rows"></param>
   /// <returns></returns>
   public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows );

   /// <summary>
   /// Writes the rows with the specified write options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="rows"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options );

   /// <summary>
   /// Executes the query and returns the result with the default query options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="query"></param>
   /// <param name="db"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query );

   /// <summary>
   /// Executes the query and returns the result with the default query options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="query"></param>
   /// <param name="db"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters );

   /// <summary>
   /// Executes the query and returns the result with the specified query options.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="query"></param>
   /// <param name="db"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options );

   /// <summary>
   /// Executes the query and returns a deferred result that can be iterated over as they
   /// are returned by the database.
   /// 
   /// It does not make sense to use this method unless you are returning a big payload and
   /// have enabled chunking through InfluxQueryOptions.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="query"></param>
   /// <returns></returns>
   public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query );

   /// <summary>
   /// Executes the query and returns a deferred result that can be iterated over as they
   /// are returned by the database.
   /// 
   /// It does not make sense to use this method unless you are returning a big payload and
   /// have enabled chunking through InfluxQueryOptions.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="query"></param>
   /// <param name="parameters"></param>
   /// <returns></returns>
   public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters );

   /// <summary>
   /// Executes the query and returns a deferred result that can be iterated over as they
   /// are returned by the database.
   /// 
   /// It does not make sense to use this method unless you are returning a big payload and
   /// have enabled chunking through InfluxQueryOptions.
   /// </summary>
   /// <typeparam name="TInfluxRow"></typeparam>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="query"></param>
   /// <param name="options"></param>
   /// <returns></returns>
   public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options );

   /// <summary>
   /// Deletes data in accordance with the specified query
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="deleteQuery"></param>
   /// <returns></returns>
   public static Task<InfluxResult> DeleteAsync( this IInfluxClient client, string db, string deleteQuery );

   /// <summary>
   /// Deletes all data older than the specified timestamp.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="to"></param>
   /// <returns></returns>
   public static Task<InfluxResult> DeleteOlderThanAsync( this IInfluxClient client, string db, string measurementName, DateTime to );

   /// <summary>
   /// Deletes all data in the specified range.
   /// </summary>
   /// <param name="client">The IInfluxClient that performs operation.</param>
   /// <param name="db"></param>
   /// <param name="measurementName"></param>
   /// <param name="from"></param>
   /// <param name="to"></param>
   /// <returns></returns>
   public static Task<InfluxResult> DeleteRangeAsync( this IInfluxClient client, string db, string measurementName, DateTime from, DateTime to );

   #endregion
}
```

To get an exact indication for what each of the parameters are refer to the documentation page provided by influxDB:
 * https://docs.influxdata.com/influxdb/v1.0/query_language/data_exploration/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/schema_exploration/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/database_management/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/continuous_queries/

