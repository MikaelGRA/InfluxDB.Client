# InfluxDB.Client

This library makes it easy to be a client for InfluxDB on .NET!

## Installation

Nuget package to come...

## Reading/Writing

The library exposes all operations on InfluxDB and can be used for reading/writing data to/from in two primary ways:
 * Using your own POCO classes.
 * Using dynamic classes.

### Using your own POCO classes.

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
 * 1 property with the type DateTime as the timestamp used in InfluxDB by adding the [InfluxTimestamp] attribute.
 * 0-* properties with the type string or a user-defined enum with the [InfluxTag] attribute that InfluxDB will use as indexed tags.
 * 1-* properties with the type string, long, double, bool, DateTime or a user-defined enum with the [InfluxField] attribute that InfluxDB will use as fields.

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
   var resultSet = await _client.ReadAsync<ComputerInfo>( $"SELECT * FROM myMeasurementName", "mydb" );
   
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

The goal is that there will be an linq-to-influx provider that allows you to use LINQ to execute queries. But for now, you must write the queries yourself.

### Using dynamic classes

POCO classes does not fit every use-case. This becomes obvious once you are implementing a system and you don't know what the fields/tags will be at compile time. In this case you must use dynamic classes.

In order for this to work, you must use the interfacae IInfluxRow that specifies reading/writing methods for tags and fields. This library already includes one implementatioon of this interfaces that uses dictionaries and supports the DLR. This class is called DynamicInfluxRow. 

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

Do note, that if you use dynamic classes, user-defined enums are not supported, as there is no way to differentiate between a string and an enum.

Here's how to query from the database:
```c#
public async Task Should_Query_Dynamic_Data()
{
   var resultSet = await _client.ReadAsync<DynamicInfluxRow>( $"SELECT * FROM myMeasurementName", "mydb" );
   
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
1. Operations that does not return anything.
2. Operations that returns an InfluxResult<TInfluxRow>, such as SHOW USERS, SHOW DATABASES, etc.. You can think of these operations as statically defined "Read" operations that queries the database, since they return the same types as ordinary queries.

All of these operations can be seen here:

```c#
      #region Ping

      public Task<InfluxPingResult> PingAsync()

      public Task<InfluxPingResult> PingAsync( int secondsToWaitForLeader )

      #endregion

      #region System Monitoring

      public async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>()

      public async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>()

      public async Task<InfluxResult<ShardRow>> ShowShards()

      #endregion
      
      #region Authentication and Authorization

      public Task CreateAdminUserAsync( string username, string password )

      public Task CreateUserAsync( string username, string password )

      public Task GrantAdminPrivilegesAsync( string username )

      public Task GrantPrivilegeAsync( DatabasePriviledge privilege, string db, string username )

      public Task RevokeAdminPrivilegesAsync( string username )

      public Task RevokePrivilegeAsync( DatabasePriviledge privilege, string db, string username )

      public Task SetPasswordAsync( string username, string password )

      public Task DropUserAsync( string username )

      public async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username )

      #endregion

      #region Database Management

      public Task CreateDatabaseIfNotExistsAsync( string db )

      public Task CreateDatabaseAsync( string db )

      public Task DropDatabaseIfExistsAsync( string db )

      public Task DropDatabaseAsync( string db )

      public Task DropSeries( string db, string measurementName )

      public Task DropSeries( string db, string measurementName, string where )

      public Task DropMeasurementAsync( string measurementName, string db )

      public Task CreateRetensionPolicyAsync( string policyName, string db, string duration, int replicationLevel, bool isDefault )

      public Task ModifyRetensionPolicyAsync( string policyName, string db, string duration, int replicationLevel, bool isDefault )

      public Task DropRetentionPolicyAsync( string policyName, string db )

      #endregion

      #region Continous Queries

      public async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( string db )

      public Task CreateContinuousQuery( string name, string db, string continuousQuery )
      
      public Task DropContinuousQuery( string name, string db )

      #endregion

      #region Schema Exploration

      public async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync()
      
      public async Task<InfluxResult<RetentionPolicyRow>> ShowRetensionPoliciesAsync( string db )

      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db )

      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName )

      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName, string where )

      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db )

      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string withMeasurement )

      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string withMeasurement, string where )

      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db )

      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db, string measurementName )

      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey )

      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey, string measurementName )

      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db )

      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db, string measurementName )

      #endregion
```

Finally if you need to execute a custom operation or multiple management operations at once, you can use one of the following methods:

```c#
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db )

      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery )

      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db )

      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery )
```

## Error handling

In case an error occurrs, an InfluxException will be thrown. Catch this and inspect the Message to get an indication of what went wrong. However, if you execute multiple operations at once, and an error occurrs, you will have to inspect the ErrorMessage on the InfluxResult that is returned instead. This is because the other operation may have succeeded.
