using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client
{
    /// <summary>
    /// Extension method for IInfluxClient.
    /// </summary>
    public static class InfluxClientExtensions
   {
        #region Raw Operations

        /// <summary>
        /// Executes an arbitrary command that returns a table as a result.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, string db , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, db, null, null, cancellationToken);
      }

        /// <summary>
        /// Executes an arbitrary command or query that returns a table as a result.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, null, null, null, cancellationToken);
      }

        /// <summary>
        /// Executes an arbitrary command or query that returns a table as a result.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, object parameters , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, null, parameters, null, cancellationToken);
      }

        /// <summary>
        /// Executes an arbitrary command that does not return a table.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, string db , CancellationToken cancellationToken = default)
      {
         return client.ExecuteOperationAsync( commandOrQuery, db, null, null, cancellationToken);
      }

        /// <summary>
        /// Executes an arbitrary command that does not return a table.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery , CancellationToken cancellationToken = default)
      {
         return client.ExecuteOperationAsync( commandOrQuery, null, null, null, cancellationToken);
      }

        /// <summary>
        /// Executes an arbitrary command that does not return a table.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, object parameters , CancellationToken cancellationToken = default)
      {
         return client.ExecuteOperationAsync( commandOrQuery, null, parameters, null, cancellationToken);
      }

        #endregion

        #region System Monitoring

        /// <summary>
        /// Shows InfluxDB stats.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>( this IInfluxClient client , CancellationToken cancellationToken = default)
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW STATS", cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// Shows InfluxDB diagnostics.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>( this IInfluxClient client , CancellationToken cancellationToken = default)
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW DIAGNOSTICS", cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// Shows Shards.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<ShardRow>> ShowShards( this IInfluxClient client , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<ShardRow>( $"SHOW SHARDS", cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        #endregion

        #region Authentication and Authorization

        /// <summary>
        /// CREATE a new admin user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet> CreateAdminUserAsync( this IInfluxClient client, string username, string password , CancellationToken cancellationToken = default)
      {
         return client.ExecuteOperationAsync( $"CREATE USER {username} WITH PASSWORD '{password}' WITH ALL PRIVILEGES", cancellationToken);
      }

        /// <summary>
        /// CREATE a new non-admin user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> CreateUserAsync( this IInfluxClient client, string username, string password , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE USER {username} WITH PASSWORD '{password}'", cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// GRANT administrative privileges to an existing user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> GrantAdminPrivilegesAsync( this IInfluxClient client, string username , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"GRANT ALL PRIVILEGES TO {username}", cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// GRANT READ, WRITE or ALL database privileges to an existing user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="privilege">The privilege.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> GrantPrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"GRANT {GetPrivilege( privilege )} ON \"{db}\" TO {username}", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// REVOKE administrative privileges from an admin user
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> RevokeAdminPrivilegesAsync( this IInfluxClient client, string username , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"REVOKE ALL PRIVILEGES FROM {username}", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="privilege">The privilege.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> RevokePrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"REVOKE {GetPrivilege( privilege )} ON \"{db}\" FROM {username}", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// SET a user’s password.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> SetPasswordAsync( this IInfluxClient client, string username, string password , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"SET PASSWORD FOR {username} = '{password}'", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// DROP a user.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropUserAsync( this IInfluxClient client, string username , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP USER {username}", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// SHOW all existing users and their admin status.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<UserRow>> ShowUsersAsync( this IInfluxClient client , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<UserRow>( $"SHOW USERS", cancellationToken ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// SHOW a user’s database privileges.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( this IInfluxClient client, string username , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<GrantsRow>( $"SHOW GRANTS FOR {username}", cancellationToken ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// Gets the privilege.
        /// </summary>
        /// <param name="privilege">The privilege.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid value. - privilege</exception>
        private static string GetPrivilege( DatabasePriviledge privilege )
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
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> CreateDatabaseAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE DATABASE \"{db}\"", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Delete a database with DROP DATABASE
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropDatabaseAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP DATABASE \"{db}\"", cancellationToken ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Delete series with DROP SERIES
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP SERIES FROM \"{measurementName}\"", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Delete series with DROP SERIES
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="where">The where.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName, string where , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP SERIES FROM \"{measurementName}\" WHERE {where}", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Delete measurements with DROP MEASUREMENT
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropMeasurementAsync( this IInfluxClient client, string db, string measurementName , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP MEASUREMENT \"{measurementName}\"", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Create retention policies with CREATE RETENTION POLICY
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="replicationLevel">The replication level.</param>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}", 
             cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Create retention policies with CREATE RETENTION POLICY
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="replicationLevel">The replication level.</param>
        /// <param name="shardGroupDuration">Duration of the shard group.</param>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetShardGroupDuration( shardGroupDuration )} {GetDefault( isDefault )}",
             cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Modify retention policies with ALTER RETENTION POLICY
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="replicationLevel">The replication level.</param>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync( 
             $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}", 
             cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Modify retention policies with ALTER RETENTION POLICY
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="replicationLevel">The replication level.</param>
        /// <param name="shardGroupDuration">Duration of the shard group.</param>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetShardGroupDuration( shardGroupDuration )} {GetDefault( isDefault )}", 
             cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Delete retention policies with DROP RETENTION POLICY
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="policyName">Name of the policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropRetentionPolicyAsync( this IInfluxClient client, string db, string policyName , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"DROP RETENTION POLICY \"{policyName}\" ON \"{db}\"",
             cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <param name="isDefault">if set to <c>true</c> [is default].</param>
        /// <returns></returns>
        private static string GetDefault( bool isDefault )
      {
         return isDefault ? "DEFAULT" : string.Empty;
      }

        /// <summary>
        /// Gets the duration of the shard group.
        /// </summary>
        /// <param name="shardGroupDuration">Duration of the shard group.</param>
        /// <returns></returns>
        private static string GetShardGroupDuration( string shardGroupDuration )
      {
         return string.IsNullOrWhiteSpace( shardGroupDuration ) ? string.Empty : $"SHARD DURATION {shardGroupDuration}";
      }

        #endregion

        #region Continous Queries

        /// <summary>
        /// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<ContinuousQueryRow>( "SHOW CONTINUOUS QUERIES", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// Creates a continuous query.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="name">The name.</param>
        /// <param name="continuousQuery">The continuous query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> CreateContinuousQuery( this IInfluxClient client, string db, string name, string continuousQuery , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"CREATE CONTINUOUS QUERY \"{name}\" ON \"{db}\"\n{continuousQuery}", db, 
             cancellationToken: cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        /// <summary>
        /// Drops a continuous query.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="name">The name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult> DropContinuousQuery( this IInfluxClient client, string db, string name , CancellationToken cancellationToken = default)
      {
         var resultSet = await client.ExecuteOperationAsync(
             $"DROP CONTINUOUS QUERY \"{name}\" ON \"{db}\"", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

        #endregion

        #region Schema Exploration

        /// <summary>
        /// Get a list of all the databases in your system.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync( this IInfluxClient client , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<DatabaseRow>( 
             $"SHOW DATABASES", 
             cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<RetentionPolicyRow>( 
             $"SHOW RETENTION POLICIES ON \"{db}\"", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW SERIES query returns the distinct series in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( $"SHOW SERIES", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW SERIES query returns the distinct series in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( 
             $"SHOW SERIES FROM \"{measurementName}\"", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW SERIES query returns the distinct series in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="where">The where.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName, string where , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( 
             $"SHOW SERIES FROM \"{measurementName}\" WHERE {where}", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW MEASUREMENTS query returns the measurements in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( 
             "SHOW MEASUREMENTS", db, 
             cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW MEASUREMENTS query returns the measurements in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="where">The where.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db, string where , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( 
             $"SHOW MEASUREMENTS WHERE {where}", db, 
             cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW MEASUREMENTS query returns the measurements in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementRegex">The measurement regex.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( 
             $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex}", db, 
             cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW MEASUREMENTS query returns the measurements in your database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementRegex">The measurement regex.</param>
        /// <param name="where">The where.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex, string where , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( 
             $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex} WHERE {where}", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// SHOW TAG KEYS returns the tag keys associated with each measurement.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<TagKeyRow>( 
             "SHOW TAG KEYS", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// SHOW TAG KEYS returns the tag keys associated with each measurement.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db, string measurementName , InfluxQueryOptions options, CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<TagKeyRow>( 
             $"SHOW TAG KEYS FROM \"{measurementName}\"", 
             db, null, options, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="tagKey">The tag key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey , CancellationToken cancellationToken = default)
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( 
             $"SHOW TAG VALUES WITH KEY = \"{tagKey}\"", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="tagKey">The tag key.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey, string measurementName , CancellationToken cancellationToken = default)
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( 
             $"SHOW TAG VALUES FROM \"{measurementName}\" WITH KEY = \"{tagKey}\"", 
             db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="tagKey">The tag key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey , CancellationToken cancellationToken = default)
      {
         return client.ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey, cancellationToken);
      }

        /// <summary>
        /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="tagKey">The tag key.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey, string measurementName , CancellationToken cancellationToken = default)
      {
         return client.ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey, measurementName, cancellationToken);
      }


        /// <summary>
        /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<FieldKeyRow>( $"SHOW FIELD KEYS", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        /// <summary>
        /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db, string measurementName , CancellationToken cancellationToken = default)
      {
         var parserResult = await client.ExecuteOperationAsync<FieldKeyRow>( $"SHOW FIELD KEYS FROM \"{measurementName}\"", db, cancellationToken: cancellationToken).ConfigureAwait( false );
         return parserResult.Results.First();
      }

        #endregion

        #region Ping

        /// <summary>
        /// Executes a ping.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxPingResult> PingAsync( this IInfluxClient client , CancellationToken cancellationToken = default)
      {
         return client.PingAsync( null, cancellationToken);
      }

        #endregion

        #region Data Management

        /// <summary>
        /// Writes the rows with default write options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, string measurementName, IEnumerable<TInfluxRow> rows , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, measurementName, rows, client.DefaultWriteOptions, cancellationToken);
      }

        /// <summary>
        /// Writes the rows with default write options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, null, rows, client.DefaultWriteOptions, cancellationToken);
      }

        /// <summary>
        /// Writes the rows with the specified write options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, null, rows, options, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns the result with the default query options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, null, client.DefaultQueryOptions, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns the result with the default query options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, parameters, client.DefaultQueryOptions, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns the result with the specified query options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, null, options, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns a deferred result that can be iterated over as they
        /// are returned by the database.
        /// It does not make sense to use this method unless you are returning a big payload and
        /// have enabled chunking through InfluxQueryOptions.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, null, client.DefaultQueryOptions, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns a deferred result that can be iterated over as they
        /// are returned by the database.
        /// It does not make sense to use this method unless you are returning a big payload and
        /// have enabled chunking through InfluxQueryOptions.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, parameters, client.DefaultQueryOptions, cancellationToken);
      }

        /// <summary>
        /// Executes the query and returns a deferred result that can be iterated over as they
        /// are returned by the database.
        /// It does not make sense to use this method unless you are returning a big payload and
        /// have enabled chunking through InfluxQueryOptions.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options , CancellationToken cancellationToken = default)
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, null, options, cancellationToken);
      }

        /// <summary>
        /// Deletes data in accordance with the specified query
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="deleteQuery">The delete query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResult> DeleteAsync( this IInfluxClient client, string db, string deleteQuery , CancellationToken cancellationToken = default)
      {
         return client.DeleteAsync( db, deleteQuery, null, cancellationToken);
      }

        /// <summary>
        /// Deletes all data older than the specified timestamp.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="to">To.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResult> DeleteOlderThanAsync( this IInfluxClient client, string db, string measurementName, DateTime to , CancellationToken cancellationToken = default)
      {
         return client.DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE time < $to", new { to }, cancellationToken);
      }

        /// <summary>
        /// Deletes all data in the specified range.
        /// </summary>
        /// <param name="client">The IInfluxClient that performs operation.</param>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static Task<InfluxResult> DeleteRangeAsync( this IInfluxClient client, string db, string measurementName, DateTime from, DateTime to , CancellationToken cancellationToken = default)
      {
         return client.DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE $from <= time AND time < $to", new { from, to }, cancellationToken);
      }

      #endregion
   }
}
