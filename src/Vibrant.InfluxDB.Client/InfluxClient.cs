﻿using System;
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
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client
{
   /// <summary>
   /// An InfluxClient exposes all HTTP operations on InfluxDB.
   /// </summary>
   public sealed class InfluxClient : IDisposable, IInfluxClient
   {
      private readonly Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo> _seriesMetaCache;
      private readonly AuthenticationHeaderValue _authzHeader;
      private readonly HttpClient _client;
      private readonly Uri _endpoint;

      private bool _disposed;
      private bool _disposeHttpClientHandler;

      private InfluxClient( Uri endpoint, string username, string password, HttpClient client, bool disposeHttpClient )
      {
         _disposeHttpClientHandler = disposeHttpClient;
         _seriesMetaCache = new Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo>();
         _endpoint = endpoint;
         _client = client;

         DefaultWriteOptions = new InfluxWriteOptions();
         DefaultQueryOptions = new InfluxQueryOptions();
         TimestampParserRegistry = new DefaultTimestampParserRegistry();

         if( !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( password ) )
         {
            var encoding = Encoding.GetEncoding( "ISO-8859-1" );
            var credentials = username + ":" + password;
            var encodedCredentialBytes = encoding.GetBytes( credentials );
            var encodedCredentials = Convert.ToBase64String( encodedCredentialBytes );
            _authzHeader = new AuthenticationHeaderValue( "Basic", encodedCredentials );
         }
      }

      /// <summary>
      /// Constructs an InfluxClient that uses the specified credentials and HttpClient.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <param name="client"></param>
      public InfluxClient( Uri endpoint, string username, string password, HttpClient client )
         : this( endpoint, username, password, client, false )
      {

      }

      /// <summary>
      /// Constructs an InfluxClient that uses the specified credentials.
      /// </summary>
      /// <param name="endpoint"></param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      public InfluxClient( Uri endpoint, string username, string password )
         : this( endpoint, username, password, new HttpClient( new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }, true ), true )
      {
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
      /// Gets or sets the timeout for all requests made.
      /// </summary>
      public TimeSpan Timeout
      {
         get
         {
            return _client.Timeout;
         }
         set
         {
            _client.Timeout = value;
         }
      }

      /// <summary>
      /// Gets the default write options.
      /// </summary>
      public InfluxWriteOptions DefaultWriteOptions { get; private set; }

      /// <summary>
      /// Gets the default query options.
      /// </summary>
      public InfluxQueryOptions DefaultQueryOptions { get; private set; }

      /// <summary>
      /// Gets the timestamp parser registry.
      /// </summary>
      public ITimestampParserRegistry TimestampParserRegistry { get; private set; }

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
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, db, false, true, null, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db, object parameters )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, db, false, true, parameters, DefaultQueryOptions );
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
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, null, false, true, null, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, object parameters )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, null, false, true, parameters, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, db, true, null, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db, object parameters )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, db, true, parameters, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, null, true, null, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, object parameters )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, null, true, parameters, DefaultQueryOptions );
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
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW STATS", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW DIAGNOSTICS", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Shows Shards.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<ShardRow>> ShowShards()
      {
         var parserResult = await ExecuteQueryInternalAsync<ShardRow>( $"SHOW SHARDS", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
      public Task<InfluxResultSet> CreateAdminUserAsync( string username, string password )
      {
         return ExecuteQueryInternalAsync( $"CREATE USER {username} WITH PASSWORD '{password}' WITH ALL PRIVILEGES", null, true, null, DefaultQueryOptions );
      }

      /// <summary>
      /// CREATE a new non-admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateUserAsync( string username, string password )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"CREATE USER {username} WITH PASSWORD '{password}'", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// GRANT administrative privileges to an existing user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> GrantAdminPrivilegesAsync( string username )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"GRANT ALL PRIVILEGES TO {username}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// GRANT READ, WRITE or ALL database privileges to an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> GrantPrivilegeAsync( string db, DatabasePriviledge privilege, string username )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"GRANT {GetPrivilege( privilege )} ON \"{db}\" TO {username}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// REVOKE administrative privileges from an admin user
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> RevokeAdminPrivilegesAsync( string username )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"REVOKE ALL PRIVILEGES FROM {username}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> RevokePrivilegeAsync( string db, DatabasePriviledge privilege, string username )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"REVOKE {GetPrivilege( privilege )} ON \"{db}\" FROM {username}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// SET a user’s password.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public async Task<InfluxResult> SetPasswordAsync( string username, string password )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"SET PASSWORD FOR {username} = '{password}'", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// DROP a user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropUserAsync( string username )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP USER {username}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// SHOW all existing users and their admin status.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<UserRow>> ShowUsersAsync()
      {
         var parserResult = await ExecuteQueryInternalAsync<UserRow>( $"SHOW USERS", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW a user’s database privileges.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      public async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username )
      {
         var parserResult = await ExecuteQueryInternalAsync<GrantsRow>( $"SHOW GRANTS FOR {username}", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
      public async Task<InfluxResult> CreateDatabaseAsync( string db )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"CREATE DATABASE \"{db}\"", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete a database with DROP DATABASE
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropDatabaseAsync( string db )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP DATABASE \"{db}\"", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropSeries( string db, string measurementName )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP SERIES FROM \"{measurementName}\"", db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropSeries( string db, string measurementName, string where )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP SERIES FROM \"{measurementName}\" WHERE {where}", db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete measurements with DROP MEASUREMENT
      /// </summary>
      /// <param name="measurementName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropMeasurementAsync( string db, string measurementName )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP MEASUREMENT \"{measurementName}\"", db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
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
      public async Task<InfluxResult> CreateRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
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
      public async Task<InfluxResult> AlterRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropRetentionPolicyAsync( string db, string policyName )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP RETENTION POLICY \"{policyName}\" ON \"{db}\"", null, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
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
         var parserResult = await ExecuteQueryInternalAsync<ContinuousQueryRow>( "SHOW CONTINUOUS QUERIES", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Creates a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <param name="continuousQuery"></param>
      /// <returns></returns>
      public async Task<InfluxResult> CreateContinuousQuery( string db, string name, string continuousQuery )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"CREATE CONTINUOUS QUERY \"{name}\" ON \"{db}\"\n{continuousQuery}", db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DropContinuousQuery( string db, string name )
      {
         var resultSet = await ExecuteQueryInternalAsync( $"DROP CONTINUOUS QUERY \"{name}\" ON \"{db}\"", db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      #endregion

      #region Schema Exploration

      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <returns></returns>
      public async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync()
      {
         var parserResult = await ExecuteQueryInternalAsync<DatabaseRow>( $"SHOW DATABASES", null, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<RetentionPolicyRow>( $"SHOW RETENTION POLICIES ON \"{db}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<ShowSeriesRow>( $"SHOW SERIES", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES FROM \"{measurementName}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES FROM \"{measurementName}\" WHERE {where}", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( "SHOW MEASUREMENTS", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WHERE {where}", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex}", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex} WHERE {where}", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db )
      {
         var parserResult = await ExecuteQueryInternalAsync<TagKeyRow>( "SHOW TAG KEYS", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<TagKeyRow>( $"SHOW TAG KEYS FROM \"{measurementName}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW TAG VALUES WITH KEY = \"{tagKey}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey, string measurementName )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW TAG VALUES FROM \"{measurementName}\" WITH KEY = \"{tagKey}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<FieldKeyRow>( $"SHOW FIELD KEYS", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         var parserResult = await ExecuteQueryInternalAsync<FieldKeyRow>( $"SHOW FIELD KEYS FROM \"{measurementName}\"", db, false, false, null, DefaultQueryOptions ).ConfigureAwait( false );
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
         return WriteInternalAsync( db, measurementName, rows, DefaultWriteOptions );
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
         return WriteInternalAsync( db, measurementName, rows, options );
      }

      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows )
         where TInfluxRow : new()
      {
         return WriteInternalAsync( db, null, rows, DefaultWriteOptions );
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
         where TInfluxRow : new()
      {
         return WriteInternalAsync( db, null, rows, options );
      }

      private Task WriteInternalAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
         where TInfluxRow : new()
      {
         List<HttpContent> contents = new List<HttpContent>();
         foreach( var groupOfRows in rows.GroupBy( x => x.GetType() ) )
         {
            var info = MetadataCache.GetOrCreate( groupOfRows.Key );

            var c = info.CreateHttpContentFor( this, groupOfRows, measurementName, options );
            contents.Add( c );
         }

         if( contents.Count == 0 ) return TaskHelpers.CompletedTask;
         var content = contents.Count == 1 ? contents[ 0 ] : new MultiContent( contents );


         if( options.UseGzip )
         {
            content = new GzipContent( content );
         }
         return PostInternalIgnoreResultAsync( CreateWriteUrl( db, options ), content );
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
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, true, false, null, DefaultQueryOptions );
      }

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, object parameters )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, true, false, parameters, DefaultQueryOptions );
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
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, true, false, null, options );
      }

      /// <summary>
      /// Executes the query and returns the result with the specified query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="options"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, true, false, parameters, options );
      }

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
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query )
         where TInfluxRow : new()
      {
         return ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>( query, db, true, false, null, DefaultQueryOptions );
      }

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
      /// <param name="parameters"></param>
      /// <returns></returns>
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, object parameters )
         where TInfluxRow : new()
      {
         return ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>( query, db, true, false, parameters, DefaultQueryOptions );
      }

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
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>( query, db, true, false, null, options );
      }

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
      public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>( query, db, true, false, parameters, options );
      }

      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DeleteAsync( string db, string deleteQuery )
      {
         var resultSet = await ExecuteQueryInternalAsync( deleteQuery, db, true, null, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public async Task<InfluxResult> DeleteAsync( string db, string deleteQuery, object parameters )
      {
         var resultSet = await ExecuteQueryInternalAsync( deleteQuery, db, true, parameters, DefaultQueryOptions ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Deletes all data older than the specified timestamp.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task<InfluxResult> DeleteOlderThanAsync( string db, string measurementName, DateTime to )
      {
         return DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE time < $to", new { to } );
      }

      /// <summary>
      /// Deletes all data in the specified range.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public Task<InfluxResult> DeleteRangeAsync( string db, string measurementName, DateTime from, DateTime to )
      {
         return DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE $from <= time AND time < $to", new { from, to } );
      }

      #endregion

      internal async Task<DatabaseMeasurementInfo> GetMetaInformationAsync( string db, string measurementName, TimeSpan? expiration )
      {
         var key = new DatabaseMeasurementInfoKey( db, measurementName );
         DatabaseMeasurementInfo info;

         lock( _seriesMetaCache )
         {
            _seriesMetaCache.TryGetValue( key, out info );
         }

         var now = DateTime.UtcNow;
         if( info != null )
         {
            if( !expiration.HasValue ) // info never expires
            {
               return info;
            }

            if( now - info.Timestamp < expiration.Value ) // has not expired
            {
               return info;
            }
         }

         // has expired or never existed, lets retrieve it

         // get metadata information from the store
         var tagsResult = await ShowTagKeysAsync( db, measurementName ).ConfigureAwait( false );
         var tags = tagsResult.Series.FirstOrDefault()?.Rows;

         info = new DatabaseMeasurementInfo( now );
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
         var url = $"write?db={UriHelper.SafeEscapeDataString( db )}&precision={options.Precision.GetQueryParameter()}&consistency={options.Consistency.GetQueryParameter()}";
         if( !string.IsNullOrEmpty( options.RetentionPolicy ) ) url += $"&rp={options.RetentionPolicy}";
         return new Uri( _endpoint, url ).ToString();
      }

      private LongFormUrlEncodedContent CreateQueryPostContent( string commandOrQuery, string db, bool isTimeSeriesQuery, bool requireChunking, object @params, InfluxQueryOptions options )
      {
         List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>( 5 );

         if( !string.IsNullOrEmpty( db ) )
         {
            param.Add( new KeyValuePair<string, string>( "db", db ) );
         }
         if( !string.IsNullOrEmpty( commandOrQuery ) )
         {
            param.Add( new KeyValuePair<string, string>( "q", commandOrQuery ) );
         }
         if( @params != null )
         {
            param.Add( new KeyValuePair<string, string>( "params", ParamsConverter.GetParams( @params ) ) );
         }

         if( options != null )
         {
            if( options.Precision.HasValue && isTimeSeriesQuery )
            {
               param.Add( new KeyValuePair<string, string>( "epoch", options.Precision.Value.GetQueryParameter() ) );
            }
            if( options.ChunkSize.HasValue )
            {
               param.Add( new KeyValuePair<string, string>( "chunked", "true" ) );
               param.Add( new KeyValuePair<string, string>( "chunk_size", options.ChunkSize.Value.ToString( CultureInfo.InvariantCulture ) ) );
            }
         }

         // add chunking if it is not already set
         if( requireChunking && options?.ChunkSize.HasValue != true )
         {
            param.Add( new KeyValuePair<string, string>( "chunked", "true" ) );
         }

         return new LongFormUrlEncodedContent( param );
      }

      private string CreateQueryUrl( string commandOrQuery, string db, bool isTimeSeriesQuery, bool requireChunking, object @params, InfluxQueryOptions options )
      {
         var query = "query";
         char seperator = '?';

         if( !string.IsNullOrEmpty( db ) )
         {
            query += $"{seperator}db={UriHelper.SafeEscapeDataString( db )}";
            seperator = '&';
         }

         if( !string.IsNullOrEmpty( commandOrQuery ) )
         {
            query += $"{seperator}q={UriHelper.SafeEscapeDataString( commandOrQuery )}";
            seperator = '&';
         }
         if( @params != null )
         {
            query += $"{seperator}params={UriHelper.SafeEscapeDataString( ParamsConverter.GetParams( @params ) )}";
            seperator = '&';
         }

         if( options != null )
         {
            if( options.Precision.HasValue && isTimeSeriesQuery )
            {
               query += $"{seperator}epoch={options.Precision.Value.GetQueryParameter()}";
               seperator = '&';
            }

            if( options.ChunkSize.HasValue )
            {
               query += $"{seperator}chunked=true&chunk_size={options.ChunkSize.Value}";
               seperator = '&';
            }
         }

         // add chunking if it is not already set
         if( requireChunking && options?.ChunkSize.HasValue != true )
         {
            query += $"{seperator}chunked=true";
         }

         return new Uri( _endpoint, query ).ToString();
      }

      private string CreatePingUrl( int? secondsToWaitForLeader )
      {
         if( secondsToWaitForLeader.HasValue )
         {
            return new Uri( _endpoint, $"ping?wait_for_leader={secondsToWaitForLeader.Value}s" ).ToString();
         }
         else
         {
            return new Uri( _endpoint, "ping" ).ToString();
         }
      }

      private async Task<InfluxChunkedResultSet<TInfluxRow>> ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>( string query, string db, bool isTimeSeriesQuery, bool forcePost, object @params, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         var iterator = await PerformQueryInternal<TInfluxRow>( query, db, forcePost, isTimeSeriesQuery, true, @params, options ).ConfigureAwait( false );
         return new InfluxChunkedResultSet<TInfluxRow>( iterator, this, options, db );
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query, string db, bool isTimeSeriesQuery, bool forcePost, object @params, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         List<QueryResult> queryResults = await PerformQueryInternal( query, db, forcePost, isTimeSeriesQuery, false, @params, options ).ConfigureAwait( false );
         return await ResultSetFactory.CreateAsync<TInfluxRow>( this, queryResults, db, isTimeSeriesQuery, options ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet> ExecuteQueryInternalAsync( string query, string db, bool forcePost, object @params, InfluxQueryOptions options )
      {
         List<QueryResult> queryResults = await PerformQueryInternal( query, db, forcePost, false, false, @params, options ).ConfigureAwait( false );
         return ResultSetFactory.Create( queryResults );
      }

      private async Task<ContextualQueryResultIterator<TInfluxRow>> PerformQueryInternal<TInfluxRow>( string query, string db, bool forcePost, bool isTimeSeriesQuery, bool requireChunking, object @params, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         ContextualQueryResultIterator<TInfluxRow> iterator;
         if( options.UsePost )
         {
            iterator = await ExecuteHttpAsync<TInfluxRow>( HttpMethod.Post, new Uri( _endpoint, "query" ).ToString(), db, options, CreateQueryPostContent( query, db, isTimeSeriesQuery, requireChunking, @params, options ) ).ConfigureAwait( false );
         }
         else
         {
            if( forcePost )
            {
               iterator = await ExecuteHttpAsync<TInfluxRow>( HttpMethod.Post, CreateQueryUrl( query, db, isTimeSeriesQuery, requireChunking, @params, options ), db, options ).ConfigureAwait( false );
            }
            else
            {
               iterator = await ExecuteHttpAsync<TInfluxRow>( HttpMethod.Get, CreateQueryUrl( query, db, isTimeSeriesQuery, requireChunking, @params, options ), db, options ).ConfigureAwait( false );
            }
         }

         return iterator;
      }

      private async Task<List<QueryResult>> PerformQueryInternal( string query, string db, bool forcePost, bool isTimeSeriesQuery, bool requireChunking, object @params, InfluxQueryOptions options )
      {
         List<QueryResult> queryResults;
         if( options.UsePost )
         {
            queryResults = await ExecuteHttpAsync( HttpMethod.Post, new Uri( _endpoint, "query" ).ToString(), CreateQueryPostContent( query, db, isTimeSeriesQuery, requireChunking, @params, options ) ).ConfigureAwait( false );
         }
         else
         {
            if( forcePost )
            {
               queryResults = await ExecuteHttpAsync( HttpMethod.Post, CreateQueryUrl( query, db, isTimeSeriesQuery, requireChunking, @params, options ) ).ConfigureAwait( false );
            }
            else
            {
               queryResults = await ExecuteHttpAsync( HttpMethod.Get, CreateQueryUrl( query, db, isTimeSeriesQuery, requireChunking, @params, options ) ).ConfigureAwait( false );
            }
         }

         return queryResults;
      }

      private async Task<List<QueryResult>> ExecuteHttpAsync( HttpMethod method, string url, HttpContent content = null )
      {
         try
         {
            using( var request = new HttpRequestMessage( method, url ) { Content = ( method == HttpMethod.Get ? null : ( content == null ? new StringContent( "" ) : content ) ) } )
            {
               request.Headers.Authorization = _authzHeader;

               using( var response = await _client.SendAsync( request ).ConfigureAwait( false ) )
               {
                  await EnsureSuccessCode( response ).ConfigureAwait( false );
                  return await response.Content.ReadMultipleAsJsonAsync<QueryResult>().ConfigureAwait( false );
               }
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private async Task<ContextualQueryResultIterator<TInfluxRow>> ExecuteHttpAsync<TInfluxRow>( HttpMethod method, string url, string db, InfluxQueryOptions options, HttpContent content = null )
         where TInfluxRow : new()
      {
         try
         {
            using( var request = new HttpRequestMessage( method, url ) { Content = ( method == HttpMethod.Get ? null : ( content == null ? new StringContent( "" ) : content ) ) } )
            {
               request.Headers.Authorization = _authzHeader;

               var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false );
               await EnsureSuccessCode( response ).ConfigureAwait( false );

               var objectIterator = await response.Content.GetObjectIteratorAsync().ConfigureAwait( false );
               var iterator = new QueryResultIterator<TInfluxRow>( response, objectIterator, this, options, db );
               var contextualIterator = new ContextualQueryResultIterator<TInfluxRow>( iterator );
               return contextualIterator;
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
         }
      }

      private Task<InfluxPingResult> ExecutePingInternalAsync( int? secondsToWaitForLeader )
      {
         return HeadInternalAsync( CreatePingUrl( secondsToWaitForLeader ) );
      }

      private async Task<InfluxPingResult> HeadInternalAsync( string url )
      {
         try
         {
            using( var request = new HttpRequestMessage( HttpMethod.Head, url ) )
            {
               request.Headers.Authorization = _authzHeader;

               using( var response = await _client.SendAsync( request ).ConfigureAwait( false ) )
               {
                  await EnsureSuccessCode( response ).ConfigureAwait( false );
                  IEnumerable<string> version = null;
                  response.Headers.TryGetValues( "X-Influxdb-Version", out version );
                  return new InfluxPingResult { Version = version?.FirstOrDefault() ?? "unknown" };
               }
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
            {
               request.Headers.Authorization = _authzHeader;

               using( var response = await _client.SendAsync( request ).ConfigureAwait( false ) )
               {
                  await EnsureSuccessCode( response ).ConfigureAwait( false );
               }
            }
         }
         catch( HttpRequestException e )
         {
            throw new InfluxException( Errors.UnknownError, e );
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
            if( _disposeHttpClientHandler )
            {
               _client.Dispose();
            }
         }
      }

      #endregion
   }
}
