using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client
{
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
      /// <returns></returns>
      Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery );
      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, object parameters );
      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db );
      /// <summary>
      /// Executes an arbitrary command that does not return a table.
      /// </summary>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db, object parameters );
      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery ) where TInfluxRow : new();
      /// <summary>
      /// Executes an arbitrary command or query that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, object parameters ) where TInfluxRow : new();
      /// <summary>
      /// Executes an arbitrary command that returns a table as a result.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="commandOrQuery"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db ) where TInfluxRow : new();
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
      /// Modify retention policies with ALTER RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <param name="duration"></param>
      /// <param name="replicationLevel"></param>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      Task<InfluxResult> AlterRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault );
      /// <summary>
      /// CREATE a new admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      Task<InfluxResultSet> CreateAdminUserAsync( string username, string password );
      /// <summary>
      /// Creates a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <param name="continuousQuery"></param>
      /// <returns></returns>
      Task<InfluxResult> CreateContinuousQuery( string db, string name, string continuousQuery );
      /// <summary>
      /// Create a database with CREATE DATABASE.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult> CreateDatabaseAsync( string db );
      /// <summary>
      /// Create retention policies with CREATE RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <param name="duration"></param>
      /// <param name="replicationLevel"></param>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      Task<InfluxResult> CreateRetentionPolicyAsync( string db, string policyName, string duration, int replicationLevel, bool isDefault );
      /// <summary>
      /// CREATE a new non-admin user.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      Task<InfluxResult> CreateUserAsync( string username, string password );
      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult> DropContinuousQuery( string db, string name );
      /// <summary>
      /// Delete a database with DROP DATABASE
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult> DropDatabaseAsync( string db );
      /// <summary>
      /// Delete measurements with DROP MEASUREMENT
      /// </summary>
      /// <param name="measurementName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult> DropMeasurementAsync( string db, string measurementName );
      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult> DropRetentionPolicyAsync( string db, string policyName );
      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult> DropSeries( string db, string measurementName );
      /// <summary>
      /// Delete series with DROP SERIES
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      Task<InfluxResult> DropSeries( string db, string measurementName, string where );
      /// <summary>
      /// DROP a user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult> DropUserAsync( string username );
      /// <summary>
      /// GRANT administrative privileges to an existing user.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult> GrantAdminPrivilegesAsync( string username );
      /// <summary>
      /// GRANT READ, WRITE or ALL database privileges to an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult> GrantPrivilegeAsync( string db, DatabasePriviledge privilege, string username );
      /// <summary>
      /// Executes a ping.
      /// </summary>
      /// <returns></returns>
      Task<InfluxPingResult> PingAsync();
      /// <summary>
      /// Executes a ping and waits for the leader to respond.
      /// </summary>
      /// <param name="secondsToWaitForLeader"></param>
      /// <returns></returns>
      Task<InfluxPingResult> PingAsync( int secondsToWaitForLeader );
      /// <summary>
      /// REVOKE administrative privileges from an admin user
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult> RevokeAdminPrivilegesAsync( string username );
      /// <summary>
      /// REVOKE READ, WRITE, or ALL database privileges from an existing user.
      /// </summary>
      /// <param name="privilege"></param>
      /// <param name="db"></param>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult> RevokePrivilegeAsync( string db, DatabasePriviledge privilege, string username );
      /// <summary>
      /// SET a user’s password.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns></returns>
      Task<InfluxResult> SetPasswordAsync( string username, string password );
      /// <summary>
      /// To see the continuous queries you have defined, query SHOW CONTINUOUS QUERIES and InfluxDB will return the name and query for each continuous query in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<ContinuousQueryRow>> ShowContinuousQueries( string db );
      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <returns></returns>
      Task<InfluxResult<DatabaseRow>> ShowDatabasesAsync();
      /// <summary>
      /// Shows InfluxDB diagnostics.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowDiagnosticsAsync<TInfluxRow>() where TInfluxRow : IInfluxRow, new();
      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db );
      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult<FieldKeyRow>> ShowFieldKeysAsync( string db, string measurementName );
      /// <summary>
      /// SHOW a user’s database privileges.
      /// </summary>
      /// <param name="username"></param>
      /// <returns></returns>
      Task<InfluxResult<GrantsRow>> ShowGrantsAsync( string username );
      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db );
      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      Task<InfluxResult<MeasurementRow>> ShowMeasurementsAsync( string db, string where );
      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <returns></returns>
      Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex );
      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementRegex"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      Task<InfluxResult<MeasurementRow>> ShowMeasurementsWithMeasurementAsync( string db, string measurementRegex, string where );
      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<RetentionPolicyRow>> ShowRetentionPoliciesAsync( string db );
      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<ShowSeriesRow>> ShowSeriesAsync( string db );
      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName ) where TInfluxRow : new();
      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db, string measurementName, string where ) where TInfluxRow : new();
      /// <summary>
      /// Shows Shards.
      /// </summary>
      /// <returns></returns>
      Task<InfluxResult<ShardRow>> ShowShards();
      /// <summary>
      /// Shows InfluxDB stats.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowStatsAsync<TInfluxRow>() where TInfluxRow : IInfluxRow, new();
      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db );
      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult<TagKeyRow>> ShowTagKeysAsync( string db, string measurementName );
      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey ) where TInfluxRow : ITagValueRow<TValue>, new();
      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <typeparam name="TValue"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult<TInfluxRow>> ShowTagValuesAsAsync<TInfluxRow, TValue>( string db, string tagKey, string measurementName ) where TInfluxRow : ITagValueRow<TValue>, new();
      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey );
      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      Task<InfluxResult<TagValueRow>> ShowTagValuesAsync( string db, string tagKey, string measurementName );
      /// <summary>
      /// SHOW all existing users and their admin status.
      /// </summary>
      /// <returns></returns>
      Task<InfluxResult<UserRow>> ShowUsersAsync();

      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query ) where TInfluxRow : new();
      /// <summary>
      /// Executes the query and returns the result with the default query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, object parameters ) where TInfluxRow : new();
      /// <summary>
      /// Executes the query and returns the result with the specified query options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options ) where TInfluxRow : new();
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
      /// <returns></returns>
      Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query ) where TInfluxRow : new();
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
      Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, object parameters ) where TInfluxRow : new();
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
      Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, InfluxQueryOptions options ) where TInfluxRow : new();
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
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows ) where TInfluxRow : new();
      /// <summary>
      /// Writes the rows with the specified write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="rows"></param>
      /// <param name="options"></param>
      /// <returns></returns>
      Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options ) where TInfluxRow : new();
      /// <summary>
      /// Writes the rows with default write options.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="rows"></param>
      /// <returns></returns>
      Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows ) where TInfluxRow : new();
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
      /// <returns></returns>
      Task<InfluxResult> DeleteAsync( string db, string deleteQuery );
      /// <summary>
      /// Deletes data in accordance with the specified query
      /// </summary>
      /// <param name="db"></param>
      /// <param name="deleteQuery"></param>
      /// <param name="parameters"></param>
      /// <returns></returns>
      Task<InfluxResult> DeleteAsync( string db, string deleteQuery, object parameters );
      /// <summary>
      /// Deletes all data older than the specified timestamp.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      Task<InfluxResult> DeleteOlderThanAsync( string db, string measurementName, DateTime to );
      /// <summary>
      /// Deletes all data in the specified range.
      /// </summary>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      Task<InfluxResult> DeleteRangeAsync( string db, string measurementName, DateTime from, DateTime to );
   }
}