using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, string db )
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, db, null );
      }

      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery )
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, null, null );
      }

      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( this IInfluxClient client, string commandOrQuery, object parameters )
         where TInfluxRow : new()
      {
         return client.ExecuteOperationAsync<TInfluxRow>( commandOrQuery, null, parameters );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, string db )
      {
         return client.ExecuteOperationAsync( commandOrQuery, db, null );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery )
      {
         return client.ExecuteOperationAsync( commandOrQuery, null, null );
      }

      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet> ExecuteOperationAsync( this IInfluxClient client, string commandOrQuery, object parameters )
      {
         return client.ExecuteOperationAsync( commandOrQuery, null, parameters );
      }

      #endregion

      #region System Monitoring

      /// <summary>
      /// Shows InfluxDB stats.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static async Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>( this IInfluxClient client )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW STATS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Shows InfluxDB diagnostics.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static async Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>( this IInfluxClient client )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW DIAGNOSTICS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Shows Shards.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static async Task<InfluxResult<ShardRow>> ShowShards( this IInfluxClient client )
      {
         var parserResult = await client.ExecuteOperationAsync<ShardRow>( $"SHOW SHARDS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      #endregion

      #region Authentication and Authorization

      /// <summary>
      /// CREATE a new admin user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet> CreateAdminUserAsync( this IInfluxClient client, string username, string password )
      {
         return client.ExecuteOperationAsync( $"CREATE USER {username} WITH PASSWORD '{password}' WITH ALL PRIVILEGES" );
      }

      /// <summary>
      /// CREATE a new non-admin user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> CreateUserAsync( this IInfluxClient client, string username, string password )
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE USER {username} WITH PASSWORD '{password}'" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// GRANT administrative privileges to an existing user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> GrantAdminPrivilegesAsync( this IInfluxClient client, string username )
      {
         var resultSet = await client.ExecuteOperationAsync( $"GRANT ALL PRIVILEGES TO {username}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// GRANT READ, WRITE or ALL database privileges to an existing user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> GrantPrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username )
      {
         var resultSet = await client.ExecuteOperationAsync( $"GRANT {GetPrivilege( privilege )} ON \"{db}\" TO {username}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// REVOKE administrative privileges from an admin user
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> RevokeAdminPrivilegesAsync( this IInfluxClient client, string username )
      {
         var resultSet = await client.ExecuteOperationAsync( $"REVOKE ALL PRIVILEGES FROM {username}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> RevokePrivilegeAsync( this IInfluxClient client, string db, DatabasePriviledge privilege, string username )
      {
         var resultSet = await client.ExecuteOperationAsync( $"REVOKE {GetPrivilege( privilege )} ON \"{db}\" FROM {username}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// SET a user’s password.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> SetPasswordAsync( this IInfluxClient client, string username, string password )
      {
         var resultSet = await client.ExecuteOperationAsync( $"SET PASSWORD FOR {username} = '{password}'" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// DROP a user.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropUserAsync( this IInfluxClient client, string username )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP USER {username}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// SHOW all existing users and their admin status.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static async Task<InfluxResult<UserRow>> ShowUsersAsync( this IInfluxClient client )
      {
         var parserResult = await client.ExecuteOperationAsync<UserRow>( $"SHOW USERS" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW a user’s database privileges.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="username"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<GrantsRow>> ShowGrantsAsync( this IInfluxClient client, string username )
      {
         var parserResult = await client.ExecuteOperationAsync<GrantsRow>( $"SHOW GRANTS FOR {username}" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

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
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> CreateDatabaseAsync( this IInfluxClient client, string db )
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE DATABASE \"{db}\"" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete a database with DROP DATABASE
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropDatabaseAsync( this IInfluxClient client, string db )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP DATABASE \"{db}\"" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP SERIES FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropSeries( this IInfluxClient client, string db, string measurementName, string where )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP SERIES FROM \"{measurementName}\" WHERE {where}", db ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete measurements with DROP MEASUREMENT
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="measurementName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropMeasurementAsync( this IInfluxClient client, string db, string measurementName )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP MEASUREMENT \"{measurementName}\"", db ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

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
      public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault )
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

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
      public static async Task<InfluxResult> CreateRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault )
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetShardGroupDuration( shardGroupDuration )} {GetDefault( isDefault )}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

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
      public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, bool isDefault )
      {
         var resultSet = await client.ExecuteOperationAsync( $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

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
      public static async Task<InfluxResult> AlterRetentionPolicyAsync( this IInfluxClient client, string db, string policyName, string duration, int replicationLevel, string shardGroupDuration, bool isDefault )
      {
         var resultSet = await client.ExecuteOperationAsync( $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetShardGroupDuration( shardGroupDuration )} {GetDefault( isDefault )}" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropRetentionPolicyAsync( this IInfluxClient client, string db, string policyName )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP RETENTION POLICY \"{policyName}\" ON \"{db}\"" ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      private static string GetDefault( bool isDefault )
      {
         return isDefault ? "DEFAULT" : string.Empty;
      }

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
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<ContinuousQueryRow>( "SHOW CONTINUOUS QUERIES", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Creates a continuous query.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <param name="continuousQuery"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> CreateContinuousQuery( this IInfluxClient client, string db, string name, string continuousQuery )
      {
         var resultSet = await client.ExecuteOperationAsync( $"CREATE CONTINUOUS QUERY \"{name}\" ON \"{db}\"\n{continuousQuery}", db ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult> DropContinuousQuery( this IInfluxClient client, string db, string name )
      {
         var resultSet = await client.ExecuteOperationAsync( $"DROP CONTINUOUS QUERY \"{name}\" ON \"{db}\"", db ).ConfigureAwait( false );
         return resultSet.Results.FirstOrDefault();
      }

      #endregion

      #region Schema Exploration

      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static async Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync( this IInfluxClient client )
      {
         var parserResult = await client.ExecuteOperationAsync<DatabaseRow>( $"SHOW DATABASES" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<RetentionPolicyRow>( $"SHOW RETENTION POLICIES ON \"{db}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( $"SHOW SERIES", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName )
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( $"SHOW SERIES FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( this IInfluxClient client, string db, string measurementName, string where )
      {
         var parserResult = await client.ExecuteOperationAsync<ShowSeriesRow>( $"SHOW SERIES FROM \"{measurementName}\" WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( "SHOW MEASUREMENTS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( this IInfluxClient client, string db, string where )
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( $"SHOW MEASUREMENTS WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex )
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( this IInfluxClient client, string db, string measurementRegex, string where )
      {
         var parserResult = await client.ExecuteOperationAsync<MeasurementRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT =~ {measurementRegex} WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<TagKeyRow>( "SHOW TAG KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( this IInfluxClient client, string db, string measurementName )
      {
         var parserResult = await client.ExecuteOperationAsync<TagKeyRow>( $"SHOW TAG KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW TAG VALUES WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

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
      public static async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( this IInfluxClient client, string db, string tagKey, string measurementName )
         where TInfluxRow : ITagValueRow<TValue>, new()
      {
         var parserResult = await client.ExecuteOperationAsync<TInfluxRow>( $"SHOW TAG VALUES FROM \"{measurementName}\" WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey )
      {
         return client.ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey );
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public static Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( this IInfluxClient client, string db, string tagKey, string measurementName )
      {
         return client.ShowTagValuesAsAsync<TagValueRow, string>( db, tagKey, measurementName );
      }


      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db )
      {
         var parserResult = await client.ExecuteOperationAsync<FieldKeyRow>( $"SHOW FIELD KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public static async Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( this IInfluxClient client, string db, string measurementName )
      {
         var parserResult = await client.ExecuteOperationAsync<FieldKeyRow>( $"SHOW FIELD KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      #endregion

      #region Ping

      /// <summary>
      /// Executes a ping.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <returns></returns>
      public static Task<InfluxPingResult> PingAsync( this IInfluxClient client )
      {
         return client.PingAsync( null );
      }

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
      public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, string measurementName, IEnumerable<TInfluxRow> rows )
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, measurementName, rows, client.DefaultWriteOptions );
      }

      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows )
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, null, rows, client.DefaultWriteOptions );
      }

      /// <summary>
      /// Writes the rows with the specified write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public static Task WriteAsync<TInfluxRow>( this IInfluxClient client, string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options )
         where TInfluxRow : new()
      {
         return client.WriteAsync( db, null, rows, options );
      }

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query )
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, null, client.DefaultQueryOptions );
      }

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters )
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, parameters, client.DefaultQueryOptions );
      }

      /// <summary>
      /// Executes the query and returns the result with the specified query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      public static Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return client.ReadAsync<TInfluxRow>( db, query, null, options );
      }

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
      public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query )
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, null, client.DefaultQueryOptions );
      }

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
      public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, object parameters )
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, parameters, client.DefaultQueryOptions );
      }

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
      public static Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( this IInfluxClient client, string db, string query, InfluxQueryOptions options )
         where TInfluxRow : new()
      {
         return client.ReadChunkedAsync<TInfluxRow>( db, query, null, options );
      }

      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <returns></returns>
      public static Task<InfluxResult> DeleteAsync( this IInfluxClient client, string db, string deleteQuery )
      {
         return client.DeleteAsync( db, deleteQuery, null );
      }

      /// <summary>
      /// Deletes all data older than the specified timestamp.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public static Task<InfluxResult> DeleteOlderThanAsync( this IInfluxClient client, string db, string measurementName, DateTime to )
      {
         return client.DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE time < $to", new { to } );
      }

      /// <summary>
      /// Deletes all data in the specified range.
      /// </summary>
      /// <param name="client">The IInfluxClient that performs operation.</param>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public static Task<InfluxResult> DeleteRangeAsync( this IInfluxClient client, string db, string measurementName, DateTime from, DateTime to )
      {
         return client.DeleteAsync( db, $"DELETE FROM \"{measurementName}\" WHERE $from <= time AND time < $to", new { from, to } );
      }

      #endregion
   }
}
