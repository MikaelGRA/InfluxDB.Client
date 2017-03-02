using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;
using Vibrant.InfluxDB.Client.Http;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// An InfluxClient exposes all HTTP operations on InfluxDB.
   /// </summary>
   public sealed class InfluxClient : IDisposable
   {
      private readonly Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo> _seriesMetaCache;
      private readonly HttpClient _client;
      private readonly HttpClientHandler _handler;
      private bool _disposed;

      /// <summary>
      /// Constructs an InfluxClient that uses the specified credentials.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      public InfluxClient( Uri endpoint, string username, string password )
      {
         _handler = new HttpClientHandler();
         _handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

         _client = new HttpClient( _handler, false );
         _client.BaseAddress = endpoint;

         _seriesMetaCache = new Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo>();

         DefaultWriteOptions = new InfluxWriteOptions();
         DefaultQueryOptions = new InfluxQueryOptions();

         if( !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( password ) )
         {
            var encoding = Encoding.GetEncoding( "ISO-8859-1" );
            var credentials = username + ":" + password;
            var encodedCredentialBytes = encoding.GetBytes( credentials );
            var encodedCredentials = Convert.ToBase64String( encodedCredentialBytes );
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Basic", encodedCredentials );
         }
      }

      /// <summary>
      /// Constructs an InfluxClient that does not use any credentials.
      /// </summary>
      /// <param name="endpoint"></param>
      public InfluxClient( Uri endpoint )
         : this( endpoint, null, null )
      {

      }

      /// <summary>
      /// Gets the default write options.
      /// </summary>
      public InfluxWriteOptions DefaultWriteOptions { get; private set; }

      /// <summary>
      /// Gets the default query optionns.
      /// </summary>
      public InfluxQueryOptions DefaultQueryOptions { get; private set; }

      #region Raw Operations

      /// <summary>
      /// Executes an arbitrary command that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, db, false );
      }

      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, db );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery )
      {
         return ExecuteQueryInternalAsync( commandOrQuery );
      }

      #endregion

      #region Ping

      /// <summary>
      /// Executes a ping.
      /// </summary>
      /// <returns></returns>
      public Task<InfluxPingResult> PingAsync()
      {
         return ExecutePingInternalAsync( null );
      }

      /// <summary>
      /// Executes a ping and waits for the leader to respond.
      /// </summary>
      /// <param name="secondsToWaitForLeader"></param>
      /// <returns></returns>
      public Task<InfluxPingResult> PingAsync( int secondsToWaitForLeader )
      {
         return ExecutePingInternalAsync( secondsToWaitForLeader );
      }

      #endregion

      #region System Monitoring

      /// <summary>
      /// Shows InfluxDB stats.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>()
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW STATS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Shows InfluxDB diagnostics.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>()
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW DIAGNOSTICS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Shows Shards.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<ShardRow>> ShowShards()
      {
         var parserResult = await ExecuteQueryInternalAsync<ShardRow>( $"SHOW SHARDS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      #endregion

      #region Authentication and Authorization

      /// <summary>
      /// CREATE a new admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public Task CreateAdminUserAsync( string username, string password )
      {
         return ExecuteOperationWithNoResultAsync( $"CREATE USER {username} WITH PASSWORD '{password}' WITH ALL PRIVILEGES" );
      }

      /// <summary>
      /// CREATE a new non-admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public Task CreateUserAsync( string username, string password )
      {
         return ExecuteOperationWithNoResultAsync( $"CREATE USER {username} WITH PASSWORD '{password}'" );
      }

      /// <summary>
      /// GRANT administrative privileges to an existing user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public Task GrantAdminPrivilegesAsync( string username )
      {
         return ExecuteOperationWithNoResultAsync( $"GRANT ALL PRIVILEGES TO {username}" );
      }

      /// <summary>
      /// GRANT READ, WRITE or ALL database privileges to an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public Task GrantPrivilegeAsync( string db, DatabasePriviledge privilege, string username )
      {
         return ExecuteOperationWithNoResultAsync( $"GRANT {GetPrivilege( privilege )} ON \"{db}\" TO {username}" );
      }

      /// <summary>
      /// REVOKE administrative privileges from an admin user
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public Task RevokeAdminPrivilegesAsync( string username )
      {
         return ExecuteOperationWithNoResultAsync( $"REVOKE ALL PRIVILEGES FROM {username}" );
      }

      /// <summary>
      /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public Task RevokePrivilegeAsync( string db, DatabasePriviledge privilege, string username )
      {
         return ExecuteOperationWithNoResultAsync( $"REVOKE {GetPrivilege( privilege )} ON \"{db}\" FROM {username}" );
      }

      /// <summary>
      /// SET a user’s password.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public Task SetPasswordAsync( string username, string password )
      {
         return ExecuteOperationWithNoResultAsync( $"SET PASSWORD FOR {username} = '{password}'" );
      }

      /// <summary>
      /// DROP a user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public Task DropUserAsync( string username )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP USER {username}" );
      }

      /// <summary>
      /// SHOW all existing users and their admin status.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<UserRow>> ShowUsersAsync()
      {
         var parserResult = await ExecuteQueryInternalAsync<UserRow>( $"SHOW USERS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW a user’s database privileges.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username )
      {
         var parserResult = await ExecuteQueryInternalAsync<GrantsRow>( $"SHOW GRANTS FOR {username}" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      private string GetPrivilege( DatabasePriviledge privilege )
      {
         switch( privilege )
         {
            case DatabasePriviledge.Read:
               return "READ";
            case DatabasePriviledge.Write:
               return "WRITE";
            case DatabasePriviledge.All:
               return "ALL";
            default:
               throw new ArgumentException( "Invalid value.", nameof( privilege ) );
         }
      }

      #endregion

      #region Database Management

      /// <summary>
      /// Create a database with CREATE DATABASE.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task CreateDatabaseAsync( string db )
      {
         return ExecuteOperationWithNoResultAsync( $"CREATE DATABASE \"{db}\"" );
      }

      /// <summary>
      /// Delete a database with DROP DATABASE
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropDatabaseAsync( string db )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP DATABASE \"{db}\"" );
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public Task DropSeries( string db, string measurementName )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP SERIES FROM \"{measurementName}\"", db );
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public Task DropSeries( string db, string measurementName, string where )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP SERIES FROM \"{measurementName}\" WHERE {where}", db );
      }

      /// <summary>
      /// Delete measurements with DROP MEASUREMENT
      /// </summary>
      /// <param name="measurementName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropMeasurementAsync( string db, string measurementName )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP MEASUREMENT \"{measurementName}\"", db );
      }

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
      {
         return ExecuteOperationWithNoResultAsync( $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}" );
      }

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
      {
         return ExecuteOperationWithNoResultAsync( $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}" );
      }

      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropRetentionPolicyAsync( string db, string policyName )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP RETENTION POLICY \"{policyName}\" ON \"{db}\"" );
      }

      private string GetDefault( bool isDefault )
      {
         return isDefault ? "DEFAULT" : string.Empty;
      }

      #endregion

      #region Continous Queries

      /// <summary>
      /// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<ContinuousQueryRow>( "SHOW CONTINUOUS QUERIES", db, false ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Creates a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <param name="continuousQuery"></param>
      /// <returns></returns>
      public Task CreateContinuousQuery( string db, string name, string continuousQuery )
      {
         return ExecuteQueryInternalAsync( $"CREATE CONTINUOUS QUERY \"{name}\" ON \"{db}\"\n{continuousQuery}", db );
      }

      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropContinuousQuery( string db, string name )
      {
         return ExecuteQueryInternalAsync( $"DROP CONTINUOUS QUERY \"{name}\" ON \"{db}\"", db );
      }

      #endregion

      #region Schema Exploration

      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync()
      {
         var parserResult = await ExecuteQueryInternalAsync<DatabaseRow>( $"SHOW DATABASES" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<RetentionPolicyRow>( $"SHOW RETENTION POLICIES ON \"{db}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<ShowSeriesRow>( $"SHOW SERIES", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName )
         where TInfluxRow : new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName, string where )
         where TInfluxRow : new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES FROM \"{measurementName}\" WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( "SHOW MEASUREMENTS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string where )
      {
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex )
      {
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex, string where )
      {
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex} WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<TagKeyRow>( "SHOW TAG KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db, string measurementName )
      {
         var parserResult = await ExecuteQueryInternalAsync<TagKeyRow>( $"SHOW TAG KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW TAG VALUES WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey, string measurementName )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW TAG VALUES FROM \"{measurementName}\" WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey )
      {
         return ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey );
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey, string measurementName )
      {
         return ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey, measurementName );
      }


      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<FieldKeyRow>( $"SHOW FIELD KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db, string measurementName )
      {
         var parserResult = await ExecuteQueryInternalAsync<FieldKeyRow>( $"SHOW FIELD KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

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
         where TInfluxRow : new()
      {
         return WriteAsync( db, x => measurementName, rows, DefaultWriteOptions );
      }

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
         where TInfluxRow : new()
      {
         return WriteAsync( db, x => measurementName, rows, options );
      }

      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows )
         where TInfluxRow : IHaveMeasurementName, new()
      {
         return WriteAsync( db, x => x.MeasurementName, rows, DefaultWriteOptions );
      }

      /// <summary>
      /// Writes the rows with the specified write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
         where TInfluxRow : IHaveMeasurementName, new()
      {
         return WriteAsync( db, x => x.MeasurementName, rows, options );
      }

      private Task WriteAsync<TInfluxRow>( string db, Func<TInfluxRow, string> getMeasurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
         where TInfluxRow : new()
      {
         return PostInternalIgnoreResultAsync( CreateWriteUrl( db, options ), new InfluxRowContent<TInfluxRow>( rows, getMeasurementName, options.Precision ) );
      }

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes the query and returns the result with the specified query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, options );
      }

      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <returns></returns>
      public Task DeleteAsync( string db, string deleteQuery )
      {
         return ExecuteQueryInternalAsync( deleteQuery, db );
      }

      /// <summary>
      /// Deletes all data older than the specified timestamp.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task DeleteOlderThanAsync( string db, string measurementName, DateTime to )
      {
         return DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE time < '{to.ToIso8601()}'" );
      }

      /// <summary>
      /// Deletes all data in the specified range.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task DeleteRangeAsync( string  db, string measurementName, DateTime from, DateTime to )
      {
         return DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE '{from.ToIso8601()}' <= time AND time < '{to.ToIso8601()}'" );
      }

      #endregion

      internal async Task<DatabaseMeasurementInfo> GetMetaInformationAsync( string db, string measurementName, bool forceRefresh )
      {
         var key = new DatabaseMeasurementInfoKey( db, measurementName );
         DatabaseMeasurementInfo info;

         if( !forceRefresh )
         {
            lock( _seriesMetaCache )
            {
               if( _seriesMetaCache.TryGetValue( key, out info ) )
               {
                  return info;
               }
            }
         }

         // get metadata information from the store
         var fieldTask = ShowFieldKeysAsync( db, measurementName );
         var tagTask = ShowTagKeysAsync( db, measurementName );
         await Task.WhenAll( fieldTask, tagTask ).ConfigureAwait( false );

         var fields = fieldTask.Result.Series.FirstOrDefault()?.Rows;
         var tags = tagTask.Result.Series.FirstOrDefault()?.Rows;

         info = new DatabaseMeasurementInfo();
         if( fields != null )
         {
            foreach( var row in fields )
            {
               info.Fields.Add( row.FieldKey );
            }
         }
         if( tags != null )
         {
            foreach( var row in tags )
            {
               info.Tags.Add( row.TagKey );
            }
         }

         lock( _seriesMetaCache )
         {
            _seriesMetaCache[ key ] = info;
         }

         return info;
      }
      private string CreateWriteUrl( string db, InfluxWriteOptions options )
      {
         return $"write?db={Uri.EscapeDataString( db )}&precision={options.Precision.GetQueryParameter()}&consistency={options.Consistency.GetQueryParameter()}";
      }

      private string CreateQueryUrl( string commandOrQuery, string db, InfluxQueryOptions options )
      {
         if( options.ChunkSize.HasValue )
         {
            return $"query?db={Uri.EscapeDataString( db )}&q={Uri.EscapeDataString( commandOrQuery )}&epoch={options.Precision.GetQueryParameter()}&chunk_size={options.ChunkSize.Value}";
         }
         else
         {
            return $"query?db={Uri.EscapeDataString( db )}&q={Uri.EscapeDataString( commandOrQuery )}&epoch={options.Precision.GetQueryParameter()}";
         }
      }

      private string CreateQueryUrl( string commandOrQuery, string db )
      {
         return $"query?db={Uri.EscapeDataString( db )}&q={Uri.EscapeDataString( commandOrQuery )}";
      }

      private string CreateQueryUrl( string commandOrQuery )
      {
         return $"query?q={Uri.EscapeDataString( commandOrQuery )}";
      }

      private string CreatePingUrl( int? secondsToWaitForLeader )
      {
         if( secondsToWaitForLeader.HasValue )
         {
            return $"ping?wait_for_leader={secondsToWaitForLeader.Value}s";
         }
         else
         {
            return "ping";
         }
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query, string db, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         var queryResult = await GetInternalAsync( CreateQueryUrl( query, db, options ), true ).ConfigureAwait( false );
         return await ResultSetFactory.CreateAsync<TInfluxRow>( this, queryResult, db, false ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query, string db, bool isMeasurementQuery = false )
         where TInfluxRow : new()
      {
         var queryResult = await GetInternalAsync( CreateQueryUrl( query, db ), isMeasurementQuery ).ConfigureAwait( false );
         return await ResultSetFactory.CreateAsync<TInfluxRow>( this, queryResult, db, !isMeasurementQuery ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query )
         where TInfluxRow : new()
      {
         var queryResult = await GetInternalAsync( CreateQueryUrl( query ), false ).ConfigureAwait( false );
         return await ResultSetFactory.CreateAsync<TInfluxRow>( this, queryResult, null, false ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet> ExecuteQueryInternalAsync( string query, string db )
      {
         var queryResult = await PostInternalAsync( CreateQueryUrl( query, db ), false ).ConfigureAwait( false );
         return ResultSetFactory.Create( queryResult );
      }

      private async Task<InfluxResultSet> ExecuteQueryInternalAsync( string query )
      {
         var queryResult = await GetInternalAsync( CreateQueryUrl( query ), false ).ConfigureAwait( false );
         return ResultSetFactory.Create( queryResult );
      }

      private Task<InfluxPingResult> ExecutePingInternalAsync( int? secondsToWaitForLeader )
      {
         return HeadInternalAsync( CreatePingUrl( secondsToWaitForLeader ) );
      }

      private async Task ExecuteOperationWithNoResultAsync( string query, string db )
      {
         await GetInternalIgnoreResultAsync( CreateQueryUrl( query, db ) ).ConfigureAwait( false );
      }

      private async Task ExecuteOperationWithNoResultAsync( string query )
      {
         await PostInternalIgnoreResultAsync( CreateQueryUrl( query ) ).ConfigureAwait( false );
      }

      private async Task<InfluxPingResult> HeadInternalAsync( string url )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Head, url ) )
            using( var response = await _client.SendAsync( request ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
               IEnumerable<string> version = null;
               response.Headers.TryGetValues( "X-Influxdb-Version", out version );
               return new InfluxPingResult { Version = version?.FirstOrDefault() ?? "unknown" };
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task<QueryResult> GetInternalAsync( string url, bool isMeasurementsQuery )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Get, url ) )
            using( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
               var queryResult = await response.Content.ReadAsJsonAsync<QueryResult>().ConfigureAwait( false );
               EnsureValidQueryResult( queryResult, isMeasurementsQuery );
               return queryResult;
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task GetInternalIgnoreResultAsync( string url )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Get, url ) )
            using( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );

               // since we are ignoring the result, we dont return anything
               // but we still need to check what is being returned
               if( response.StatusCode == HttpStatusCode.OK )
               {
                  var queryResult = await response.Content.ReadAsJsonAsync<QueryResult>().ConfigureAwait( false );
                  EnsureValidQueryResult( queryResult, false );
               }
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task<QueryResult> PostInternalAsync( string url, bool isMeasurementsQuery )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Post, url ) { Content = new StringContent( "" ) } )
            using( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
               var queryResult = await response.Content.ReadAsJsonAsync<QueryResult>().ConfigureAwait( false );
               EnsureValidQueryResult( queryResult, isMeasurementsQuery );
               return queryResult;
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task PostInternalIgnoreResultAsync( string url, HttpContent content )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Post, url ) { Content = content } )
            using( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task PostInternalIgnoreResultAsync( string url )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Post, url ) { Content = new StringContent( "" ) } )
            using( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private void EnsureValidQueryResult( QueryResult queryResult, bool isMeasurementsQuery )
      {
         // If there is only one result, we will throw an exception
         if( queryResult.Results.Count == 1 )
         {
            var resultWrapper = queryResult.Results[ 0 ];
            if( resultWrapper.Error != null )
            {
               throw new InfluxException( resultWrapper.Error );
            }

            // COMMENT: We really should throw exception here, but we are unable to
            // distinguish between "no data" and "error query"

            //if ( isMeasurementsQuery && resultWrapper.Series == null )
            //{
            //   throw new InfluxException( Errors.UnexpectedQueryResult );
            //}
         }
      }

      private async Task EnsureSuccessCode( HttpResponseMessage response )
      {
         if( !response.IsSuccessStatusCode )
         {
            try
            {
               var errorResult = await response.Content.ReadAsJsonAsync<ErrorResult>().ConfigureAwait( false );
               if( errorResult?.Error != null )
               {
                  throw new InfluxException( errorResult.Error );
               }
               else
               {
                  response.EnsureSuccessStatusCode();
               }
            }
            catch( JsonSerializationException e )
            {
               throw new InfluxException( Errors.ParsingError, e );
            }
         }
      }

      #region IDisposable

      /// <summary>
      /// Destructor.
      /// </summary>
      ~InfluxClient()
      {
         Dispose( false );
      }

      /// <summary>
      /// Disposes the InfluxClient and the internal HttpClient that it uses.
      /// </summary>
      public void Dispose()
      {
         if( !_disposed )
         {
            Dispose( true );
            _disposed = true;
            GC.SuppressFinalize( this );
         }
      }

      private void Dispose( bool disposing )
      {
         if( disposing )
         {
            _client.Dispose();
            _handler.Dispose();
         }
      }

      #endregion
   }
}
