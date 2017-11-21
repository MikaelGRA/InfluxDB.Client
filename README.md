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
 * 0-* properties with the type string or a user-defined enum (nullables too) with the [InfluxTag] attribute that InfluxDB will use as indexed tags.
 * 1-* properties with the type string, long, ulong, int, uint, short, ushort, byte, sbyte, double, float, bool, DateTime or a user-defined enum (nullables too) with the [InfluxField] attribute that InfluxDB will use as fields.

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

This can be achieved by implemented the following interface on your POCO classes:

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

And using one of the overloads of the WriteAsync method that does not take a measurementName as argument:

```C#

      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows )
      
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
      
```

In case you want to do this with dynamic classes, you can simply use the NamedDynamicInfluxRow (which implements this interface) or implement a class that implements both IInfluxRow and IHaveMeasurementName yourself.

## Other operations

The InfluxClient also defines a host of other management operations. That can be divided up into two categories.
 * Operations that does not return anything.
 * Operations that returns an InfluxResult<TInfluxRow>, such as SHOW USERS, SHOW DATABASES, etc.. You can think of these operations as statically defined "Read" operations that queries the database, since they return the same types as ordinary queries.

The interface for all these operations can be seen below:

```c#
      #region Ping

      /// <summary>
      /// Executes a ping.
      /// </summary>
      /// <returns></returns>
      public Task<InfluxPingResult> PingAsync();

      /// <summary>
      /// Executes a ping and waits for the leader to respond.
      /// </summary>
      /// <param name="secondsToWaitForLeader"></param>
      /// <returns></returns>
      public Task<InfluxPingResult> PingAsync( int secondsToWaitForLeader );

      #endregion

      #region System Monitoring

      /// <summary>
      /// Shows InfluxDB stats.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>();

      /// <summary>
      /// Shows InfluxDB diagnostics.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>();

      /// <summary>
      /// Shows Shards.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<ShardRow>> ShowShards();

      #endregion

      #region Authentication and Authorization

      /// <summary>
      /// CREATE a new admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> CreateAdminUserAsync( string username, string password );

      /// <summary>
      /// CREATE a new non-admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateUserAsync( string username, string password );

      /// <summary>
      /// GRANT administrative privileges to an existing user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> GrantAdminPrivilegesAsync( string username );

      /// <summary>
      /// GRANT READ, WRITE or ALL database privileges to an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> GrantPrivilegeAsync( string db, DatabasePriviledge privilege, string username );

      /// <summary>
      /// REVOKE administrative privileges from an admin user
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> RevokeAdminPrivilegesAsync( string username );

      /// <summary>
      /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> RevokePrivilegeAsync( string db, DatabasePriviledge privilege, string username );

      /// <summary>
      /// SET a user’s password.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public async Task<InfluxResult> SetPasswordAsync( string username, string password );

      /// <summary>
      /// DROP a user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropUserAsync( string username );

      /// <summary>
      /// SHOW all existing users and their admin status.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<UserRow>> ShowUsersAsync();

      /// <summary>
      /// SHOW a user’s database privileges.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username );

      #endregion

      #region Database Management

      /// <summary>
      /// Create a database with CREATE DATABASE.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateDatabaseAsync( string db );

      /// <summary>
      /// Delete a database with DROP DATABASE
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropDatabaseAsync( string db );

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropSeries( string db, string measurementName );

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropSeries( string db, string measurementName, string where );

      /// <summary>
      /// Delete measurements with DROP MEASUREMENT
      /// </summary>
      /// <param name="measurementName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropMeasurementAsync( string db, string measurementName );

      /// <summary>
      /// Create retention policies with CREATE RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <param name="duration"></param>
      /// <param name="replicationLevel"></param>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault );

      /// <summary>
      /// Modify retention policies with ALTER RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <param name="duration"></param>
      /// <param name="replicationLevel"></param>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      public async Task<InfluxResult> AlterRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault );

      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropRetentionPolicyAsync( string db, string policyName );

      #endregion

      #region Continous Queries

      /// <summary>
      /// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( string db );

      /// <summary>
      /// Creates a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <param name="continuousQuery"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateContinuousQuery( string db, string name, string continuousQuery );

      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropContinuousQuery( string db, string name ); return resultSet.Results.FirstOrDefault();
      }

      #endregion

      #region Schema Exploration

      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync();

      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( string db );

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( string db );

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName );

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName, string where );

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db );

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string where );

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex );

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex, string where );

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db );

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db, string measurementName );

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey );

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey, string measurementName );

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey );

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey, string measurementName );


      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db );

      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db, string measurementName );

      #endregion

      #region Data Management

      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows );

      /// <summary>
      /// Writes the rows with the specified write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="rows"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options );

      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows );

      /// <summary>
      /// Writes the rows with the specified write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options );

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query );

      /// <summary>
      /// Executes the query and returns the result with the specified query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options );

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
      /// <returns></returns>
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query );

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
      /// <returns></returns>
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options );

      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <returns></returns>
      public Task DeleteAsync( string db, string deleteQuery );

      /// <summary>
      /// Deletes all data older than the specified timestamp.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task DeleteOlderThanAsync( string db, string measurementName, DateTime to );

      /// <summary>
      /// Deletes all data in the specified range.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task DeleteRangeAsync( string db, string measurementName, DateTime from, DateTime to );

      #endregion
      
      #region Raw Operations

      /// <summary>
      /// Executes an arbitrary command that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="command"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string command, string db );

      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="command"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string command );

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db );

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery );

      #endregion
```

To get an exact indication for what each of the parameters are refer to the documentation page provided by influxDB:
 * https://docs.influxdata.com/influxdb/v1.0/query_language/data_exploration/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/schema_exploration/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/database_management/
 * https://docs.influxdata.com/influxdb/v1.0/query_language/continuous_queries/

