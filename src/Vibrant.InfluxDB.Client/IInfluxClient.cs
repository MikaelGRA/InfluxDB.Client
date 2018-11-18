using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Rows;

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
}