using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
    /// <summary>
    /// An InfluxClient exposes all HTTP operations on InfluxDB.
    /// </summary>
    public interface IInfluxClient
   {
        /// <summary>
        /// Gets the default query optionns.
        /// </summary>
        /// <value>
        /// The default query options.
        /// </value>
        InfluxQueryOptions DefaultQueryOptions { get; }
        /// <summary>
        /// Gets the default write options.
        /// </summary>
        /// <value>
        /// The default write options.
        /// </value>
        InfluxWriteOptions DefaultWriteOptions { get; }
        /// <summary>
        /// Gets or sets the timeout for all requests made.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        TimeSpan Timeout { get; set; }
        /// <summary>
        /// Gets the timestamp parser registry.
        /// </summary>
        /// <value>
        /// The timestamp parser registry.
        /// </value>
        ITimestampParserRegistry TimestampParserRegistry { get; }

        /// <summary>
        /// Executes an arbitrary command that does not return a table.
        /// </summary>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default );
        /// <summary>
        /// Executes an arbitrary command that returns a table as a result.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default ) where TInfluxRow : new();

        /// <summary>
        /// Executes a ping and waits for the leader to respond.
        /// </summary>
        /// <param name="secondsToWaitForLeader">The seconds to wait for leader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxPingResult> PingAsync( int? secondsToWaitForLeader, CancellationToken cancellationToken = default );

        /// <summary>
        /// Executes the query and returns the result with the specified query options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default ) where TInfluxRow : new();
        /// <summary>
        /// Executes the query and returns a deferred result that can be iterated over as they
        /// are returned by the database.
        /// It does not make sense to use this method unless you are returning a big payload and
        /// have enabled chunking through InfluxQueryOptions.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="db">The database.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>( string db, string query, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default ) where TInfluxRow : new();
        /// <summary>
        /// Writes the rows with the specified write options.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="db">The database.</param>
        /// <param name="measurementName">Name of the measurement.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options, CancellationToken cancellationToken = default ) where TInfluxRow : new();
        /// <summary>
        /// Deletes data in accordance with the specified query
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="deleteQuery">The delete query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<InfluxResult> DeleteAsync( string db, string deleteQuery, object parameters, CancellationToken cancellationToken = default );
   }
}