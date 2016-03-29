# InfluxDB Client for .NET
[![Build status](https://ci.appveyor.com/api/projects/status/0rkna8pevwiv9acy/branch/master?svg=true)](https://ci.appveyor.com/project/MikaelGRA/influxdb-client/branch/master)

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

The library exposes all HTTP operations on InfluxDB (0.9.6+) and can be used for reading/writing data to/from in two primary ways:
 * Using your own POCO classes.
 * Using dynamic classes.

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
 * 1 property with the type DateTime or DateTime? as the timestamp used in InfluxDB by adding the [InfluxTimestamp] attribute.
 * 0-* properties with the type string or a user-defined enum (nullables too) with the [InfluxTag] attribute that InfluxDB will use as indexed tags.
 * 1-* properties with the type string, long, double, bool, DateTime or a user-defined enum (nullables too) with the [InfluxField] attribute that InfluxDB will use as fields.

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
   await _client.WriteAsync( "mydb", "myMeasurementName", infos );
}
```

Here's how to query from the database:
```c#
public async Task Should_Query_Typed_Data()
{
   var resultSet = await _client.ReadAsync<ComputerInfo>( "mydb", "SELECT * FROM myMeasurementName" );
   
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
   await _client.WriteAsync( "mydb", "myMeasurementName", infos );
}
```

Do note, that if you use dynamic classes, user-defined enums and DateTimes as fields/tags are not supported, as there is no way to differentiate between a string and an enum/DateTime.

Here's how to query from the database:

```c#
public async Task Should_Query_Dynamic_Data()
{
   var resultSet = await _client.ReadAsync<DynamicInfluxRow>( "mydb", "SELECT * FROM myMeasurementName" );
   
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
public Task<InfluxPingResult> PingAsync()

/// <summary>
/// Executes a ping and waits for the leader to respond.
/// </summary>
/// <param name="secondsToWaitForLeader"></param>
/// <returns></returns>
public Task<InfluxPingResult> PingAsync( int secondsToWaitForLeader )

#endregion

#region System Monitoring

/// <summary>
/// Shows InfluxDB stats.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>()

/// <summary>
/// Shows InfluxDB diagnostics.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>()

/// <summary>
/// Shows Shards.
/// </summary>
/// <returns></returns>
public async Task<InfluxResult<ShardRow>> ShowShards()

#endregion

#region Authentication and Authorization

/// <summary>
/// CREATE a new admin user.
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <returns></returns>
public Task CreateAdminUserAsync( string username, string password )

/// <summary>
/// CREATE a new non-admin user.
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <returns></returns>
public Task CreateUserAsync( string username, string password )

/// GRANT administrative privileges to an existing user.
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <returns></returns>
public Task GrantAdminPrivilegesAsync( string username )

/// <summary>
/// GRANT READ, WRITE or ALL database privileges to an existing user.
/// </summary>
/// <param name="privilege"></param>
/// <param name="db"></param>
/// <param name="username"></param>
/// <returns></returns>
public Task GrantPrivilegeAsync( string db, DatabasePriviledge privilege, string username )

/// <summary>
/// REVOKE administrative privileges from an admin user
/// </summary>
/// <param name="username"></param>
/// <returns></returns>
public Task RevokeAdminPrivilegesAsync( string username )

/// <summary>
/// REVOKE READ, WRITE, or ALL database privileges from an existing user.
/// </summary>
/// <param name="privilege"></param>
/// <param name="db"></param>
/// <param name="username"></param>
/// <returns></returns>
public Task RevokePrivilegeAsync( string db, DatabasePriviledge privilege, string username )

/// <summary>
/// SET a user’s password.
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <returns></returns>
public Task SetPasswordAsync( string username, string password )

/// <summary>
/// DROP a user.
/// </summary>
/// <param name="username"></param>
/// <returns></returns>
public Task DropUserAsync( string username )

/// <summary>
/// SHOW all existing users and their admin status.
/// </summary>
/// <returns></returns>
public async Task<InfluxResult<UserRow>> ShowUsersAsync()

/// <summary>
/// SHOW a user’s database privileges.
/// </summary>
/// <param name="username"></param>
/// <returns></returns>
public async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username )

#endregion

#region Database Management

/// <summary>
/// Create a database with CREATE DATABASE IF NOT EXISTS.
/// </summary>
/// <param name="db"></param>
/// <returns></returns>
public Task CreateDatabaseIfNotExistsAsync( string db )

/// <summary>
/// Create a database with CREATE DATABASE.
/// </summary>
/// <param name="db"></param>
/// <returns></returns>
public Task CreateDatabaseAsync( string db )

/// <summary>
/// Delete a database with DROP DATABASE IF EXUSTS,
/// </summary>
/// <param name="db"></param>
/// <returns></returns>
public Task DropDatabaseIfExistsAsync( string db )

/// <summary>
/// Delete a database with DROP DATABASE
/// </summary>
/// <param name="db"></param>
/// <returns></returns>
public Task DropDatabaseAsync( string db )

/// <summary>
/// Delete series with DROP SERIES
/// </summary>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <returns></returns>
public Task DropSeries( string db, string measurementName )

/// <summary>
/// Delete series with DROP SERIES
/// </summary>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <param name="where"></param>
/// <returns></returns>
public Task DropSeries( string db, string measurementName, string where )

/// <summary>
/// Delete measurements with DROP MEASUREMENT
/// </summary>
/// <param name="measurementName"></param>
/// <param name="db"></param>
/// <returns></returns>
public Task DropMeasurementAsync( string db, string measurementName )

/// <summary>
/// Create retention policies with CREATE RETENTION POLICY
/// </summary>
/// <param name="policyName"></param>
/// <param name="db"></param>
/// <param name="duration"></param>
/// <param name="replicationLevel"></param>
/// <param name="isDefault"></param>
/// <returns></returns>
public Task CreateRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault )

/// <summary>
/// Modify retention policies with ALTER RETENTION POLICY
/// </summary>
/// <param name="policyName"></param>
/// <param name="db"></param>
/// <param name="duration"></param>
/// <param name="replicationLevel"></param>
/// <param name="isDefault"></param>
/// <returns></returns>
public Task AlterRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault )

/// <summary>
/// Delete retention policies with DROP RETENTION POLICY
/// </summary>
/// <param name="policyName"></param>
/// <param name="db"></param>
/// <returns></returns>
public Task DropRetentionPolicyAsync( string db, string policyName )

#endregion

#region Continous Queries

/// <summary>
/// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <returns></returns>
public async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( string db )

/// <summary>
/// Creates a continuous query.
/// </summary>
/// <param name="name"></param>
/// <param name="db"></param>
/// <param name="continuousQuery"></param>
/// <returns></returns>
public Task CreateContinuousQuery( string db, string name, string continuousQuery )

/// <summary>
/// Drops a continuous query.
/// </summary>
/// <param name="name"></param>
/// <param name="db"></param>
/// <returns></returns>
public Task DropContinuousQuery( string db, string name )

#endregion

#region Schema Exploration

/// <summary>
/// Get a list of all the databases in your system.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <returns></returns>
public async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync()

/// <summary>
/// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <returns></returns>
public async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( string db )

/// <summary>
/// The SHOW SERIES query returns the distinct series in your database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db )

/// <summary>
/// The SHOW SERIES query returns the distinct series in your database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName )

/// <summary>
/// The SHOW SERIES query returns the distinct series in your database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <param name="where"></param>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName, string where )

/// <summary>
/// The SHOW MEASUREMENTS query returns the measurements in your database.
/// </summary>
/// <param name="db"></param>
/// <returns></returns>
public Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db )

/// <summary>
/// The SHOW MEASUREMENTS query returns the measurements in your database.
/// </summary>
/// <param name="db"></param>
/// <param name="where"></param>
/// <returns></returns>
public Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string where )

/// <summary>
/// The SHOW MEASUREMENTS query returns the measurements in your database.
/// </summary>
/// <param name="db"></param>
/// <param name="measurementRegex"></param>
/// <returns></returns>
public Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex )

/// <summary>
/// The SHOW MEASUREMENTS query returns the measurements in your database.
/// </summary>
/// <param name="db"></param>
/// <param name="measurementRegex"></param>
/// <param name="where"></param>
/// <returns></returns>
public Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex, string where )

/// <summary>
/// SHOW TAG KEYS returns the tag keys associated with each measurement.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <returns></returns>
public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db )

/// <summary>
/// SHOW TAG KEYS returns the tag keys associated with each measurement.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <returns></returns>
public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db, string measurementName )

/// <summary>
/// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="tagKey"></param>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey )

/// <summary>
/// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="tagKey"></param>
/// <param name="measurementName"></param>
/// <returns></returns>
public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey, string measurementName )


/// <summary>
/// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <returns></returns>
public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db )

/// <summary>
/// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <returns></returns>
public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db, string measurementName )

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
public Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows )

/// <summary>
/// Writes the rows with the specified write options.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="measurementName"></param>
/// <param name="rows"></param>
/// <param name="options"></param>
/// <returns></returns>
public Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )

/// <summary>
/// Writes the rows with default write options.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="rows"></param>
/// <returns></returns>
public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows )

/// <summary>
/// Writes the rows with the specified write options.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="db"></param>
/// <param name="rows"></param>
/// <param name="options"></param>
/// <returns></returns>
public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )

/// <summary>
/// Executes the query and returns the result with the default query options.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="query"></param>
/// <param name="db"></param>
/// <returns></returns>
public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query )

/// <summary>
/// Executes the query and returns the result with the specified query options.
/// </summary>
/// <typeparam name="TInfluxRow"></typeparam>
/// <param name="query"></param>
/// <param name="db"></param>
/// <param name="options"></param>
/// <returns></returns>
public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options )

#endregion
```

To get an exact indication for what each of the parameters are refer to the documentation page provided by influxDB:
 * https://docs.influxdata.com/influxdb/v0.9/query_language/data_exploration/
 * https://docs.influxdata.com/influxdb/v0.9/query_language/schema_exploration/
 * https://docs.influxdata.com/influxdb/v0.9/query_language/database_management/
 * https://docs.influxdata.com/influxdb/v0.9/query_language/continuous_queries/

Finally if you need to execute a custom operation or multiple management operations at once, you can use one of the following methods:

```c#
public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db )

public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery )

public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db )

public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery )
```

## Error handling

In case an error occurrs, an InfluxException will be thrown. Catch this and inspect the Message to get an indication of what went wrong. However, if you execute multiple operations at once, and an error occurrs, you will have to inspect the ErrorMessage on the InfluxResult that is returned instead. This is because the other operation may have succeeded.

**0.10.0+:** Since this version, pretty much the only queries that causes errors are malformed ones.
 * Queries such as CREATE DATABASE or CREATE RETENTION POLICY that specifies an item that already exists, will always return a successful response. This is because there is no way to differentiate between an error response and a success response. See https://github.com/influxdata/influxdb/issues/5563
