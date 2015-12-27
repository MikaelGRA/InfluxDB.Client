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
using Vibrant.InfluxDB.Client.Helpers;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Rows;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxClient
   {
      private readonly HttpClient _client;
      private readonly HttpClientHandler _handler;
      private Dictionary<DatabaseSeriesInfoKey, DatabaseSeriesInfo> _seriesMetaCache;

      public InfluxClient( Uri endpoint, string username, string password )
      {
         _handler = new HttpClientHandler();
         _client = new HttpClient( _handler, true );
         _client.BaseAddress = endpoint;

         _seriesMetaCache = new Dictionary<DatabaseSeriesInfoKey, DatabaseSeriesInfo>();

         if ( !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( password ) )
         {
            var encoding = Encoding.GetEncoding( "ISO-8859-1" );
            var credentials = username + ":" + password;
            var encodedCredentialBytes = encoding.GetBytes( credentials );
            var encodedCredentials = Convert.ToBase64String( encodedCredentialBytes );
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Basic", encodedCredentials );
         }
      }

      public InfluxClient( Uri endpoint )
         : this( endpoint, null, null )
      {

      }

      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery, string db )
         where TInfluxRow : IInfluxRow, new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery, db, false );
      }

      public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>( string commandOrQuery )
         where TInfluxRow : IInfluxRow, new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( commandOrQuery );
      }

      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery, string db )
      {
         return ExecuteQueryInternalAsync( commandOrQuery, db );
      }

      public Task<InfluxResultSet> ExecuteOperationAsync( string commandOrQuery )
      {
         return ExecuteQueryInternalAsync( commandOrQuery );
      }

      #region Database Management

      /// <summary>
      /// Create a database with CREATE DATABASE IF NOT EXISTS.
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task CreateDatabaseIfNotExistsAsync( string db )
      {
         return ExecuteOperationWithNoResultAsync( $"CREATE DATABASE IF NOT EXISTS \"{db}\"" );
      }

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
      /// Delete a database with DROP DATABASE IF EXUSTS,
      /// </summary>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropDatabaseIfExistsAsync( string db )
      {
         return ExecuteOperationWithNoResultAsync( $"DROP DATABASE IF EXISTS \"{db}\"" );
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
      public Task DropMeasurementAsync( string measurementName, string db )
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
      public Task CreateRetensionPolicyAsync( string policyName, string db, string duration, int replicationLevel, bool isDefault )
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
      public Task ModifyRetensionPolicyAsync( string policyName, string db, string duration, int replicationLevel, bool isDefault )
      {
         return ExecuteOperationWithNoResultAsync( $"ALTER RETENTION POLICY \"{policyName}\" ON \"{db}\" DURATION {duration} REPLICATION {replicationLevel} {GetDefault( isDefault )}" );
      }

      /// <summary>
      /// Delete retention policies with DROP RETENTION POLICY
      /// </summary>
      /// <param name="policyName"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DeleteRetentionPolicyAsync( string policyName, string db )
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
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ListContinuousQueries<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( "SHOW CONTINUOUS QUERIES", db, false ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// Drops a continuous query.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task DropContinuousQuery( string name, string db )
      {
         return ExecuteQueryInternalAsync( $"DROP CONTINUOUS QUERY \"{name}\" ON \"{db}\"", db );
      }

      #endregion

      #region Schema Exploration

      /// <summary>
      /// Get a list of all the databases in your system.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowDatabasesAsync<TInfluxRow>()
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW DATABASES" ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW RETENTION POLICIES query lists the existing retention policies on a given database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowRetensionPoliciesAsync<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW RETENTION POLICIES ON \"{db}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW SERIES query returns the distinct series in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowSeriesAsync<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES", db ).ConfigureAwait( false );
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
         where TInfluxRow : IInfluxRow, new()
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
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW SERIES FROM \"{measurementName}\" WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowMeasurementsAsync<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( "SHOW MEASUREMENTS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="withMeasurement"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowMeasurementsAsync<TInfluxRow>( string db, string withMeasurement )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT {withMeasurement}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW MEASUREMENTS query returns the measurements in your database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="withMeasurement"></param>
      /// <param name="where"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowMeasurementsAsync<TInfluxRow>( string db, string withMeasurement, string where )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW MEASUREMENTS WITH MEASUREMENT {withMeasurement} WHERE {where}", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagsKeysAsync<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( "SHOW TAG KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// SHOW TAG KEYS returns the tag keys associated with each measurement.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagsKeysAsync<TInfluxRow>( string db, string measurementName )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW TAG KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW TAG VALUES query returns the set of tag values for a specific tag key across all measurements in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="tagKey"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW FIELD VALUES WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
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
      public async Task<InfluxResult<TInfluxRow>> ShowTagValuesAsync<TInfluxRow>( string db, string tagKey, string measurementName )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW FIELD VALUES FROM \"{measurementName}\" WITH KEY = \"{tagKey}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }


      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowFieldKeysAsync<TInfluxRow>( string db )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW FIELD KEYS", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      /// <summary>
      /// The SHOW FIELD KEYS query returns the field keys across each measurement in the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <returns></returns>
      public async Task<InfluxResult<TInfluxRow>> ShowFieldKeysAsync<TInfluxRow>( string db, string measurementName )
         where TInfluxRow : IInfluxRow, new()
      {
         var parserResult = await ExecuteQueryInternalAsync<TInfluxRow>( $"SHOW FIELD KEYS FROM \"{measurementName}\"", db ).ConfigureAwait( false );
         return parserResult.Results.First();
      }

      #endregion

      #region Data Management

      /// <summary>
      /// Writes the rows to the measurement.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="measurementName"></param>
      /// <param name="dataPoints"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, string measurementName, IEnumerable<TInfluxRow> dataPoints, TimestampPrecision precision, Consistency consistency )
         where TInfluxRow : IInfluxRow, new()
      {
         return WriteAsync( db, x => measurementName, dataPoints, precision, consistency );
      }

      /// <summary>
      /// Writes the rows. The rows themselves must define their own measurement name
      /// by implementing IMeasurementInfluxRow.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="db"></param>
      /// <param name="dataPoints"></param>
      /// <returns></returns>
      public Task WriteAsync<TInfluxRow>( string db, IEnumerable<TInfluxRow> dataPoints, TimestampPrecision precision, Consistency consistency )
         where TInfluxRow : IMeasurementInfluxRow, new()
      {
         return WriteAsync( db, x => x.SeriesName, dataPoints, precision, consistency );
      }

      private Task WriteAsync<TInfluxRow>( string db, Func<TInfluxRow, string> getMeasurementName, IEnumerable<TInfluxRow> dataPoints, TimestampPrecision precision, Consistency consistency )
         where TInfluxRow : IInfluxRow, new()
      {
         return PostInternalIgnoreResultAsync( CreateWriteUrl( db, precision.GetQueryParameter(), consistency.GetQueryParameter() ), new InfluxRowContent<TInfluxRow>( dataPoints, getMeasurementName, precision ) );
      }

      /// <summary>
      /// Reads rows from the database.
      /// </summary>
      /// <typeparam name="TInfluxRow"></typeparam>
      /// <param name="query"></param>
      /// <param name="db"></param>
      /// <returns></returns>
      public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>( string query, string db )
         where TInfluxRow : IInfluxRow, new()
      {
         return ExecuteQueryInternalAsync<TInfluxRow>( query, db, TimestampPrecision.Nanosecond );
      }

      #endregion

      internal async Task<DatabaseSeriesInfo> GetMetaInformationAsync( string db, string measurementName, bool forceRefresh )
      {
         var key = new DatabaseSeriesInfoKey( db, measurementName );
         DatabaseSeriesInfo info;

         if ( !forceRefresh )
         {
            lock ( _seriesMetaCache )
            {
               if ( _seriesMetaCache.TryGetValue( key, out info ) )
               {
                  return info;
               }
            }
         }

         // get metadata information from the store
         var fieldTask = ShowFieldKeysAsync<FieldKeyRow>( db, measurementName );
         var tagTask = ShowTagsKeysAsync<TagKeyRow>( db, measurementName );
         await Task.WhenAll( fieldTask, tagTask ).ConfigureAwait( false );

         var fields = fieldTask.Result.Series.First().Rows;
         var tags = tagTask.Result.Series.First().Rows;

         info = new DatabaseSeriesInfo();
         foreach ( var row in fields )
         {
            info.Fields.Add( row.FieldKey );
         }
         foreach ( var row in tags )
         {
            info.Tags.Add( row.TagKey );
         }

         lock ( _seriesMetaCache )
         {
            _seriesMetaCache[ key ] = info;
         }

         return info;
      }
      private string CreateWriteUrl( string db, string precision, string consistency )
      {
         return $"write?db={Uri.EscapeDataString( db )}&precision={precision}&consistency={consistency}";
      }

      private string CreateQueryOrCommandUrl( string path, string commandOrQuery, string db, string precision )
      {
         return $"{path}?db={Uri.EscapeDataString( db )}&q={Uri.EscapeDataString( commandOrQuery )}&precision={precision}";
      }

      private string CreateQueryOrCommandUrl( string path, string commandOrQuery, string db )
      {
         return $"{path}?db={Uri.EscapeDataString( db )}&q={Uri.EscapeDataString( commandOrQuery )}";
      }

      private string CreateQueryOrCommandUrl( string path, string commandOrQuery )
      {
         return $"{path}?q={Uri.EscapeDataString( commandOrQuery )}";
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query, string db, TimestampPrecision precision )
         where TInfluxRow : IInfluxRow, new()
      {
         var queryResult = await GetInternalAsync( CreateQueryOrCommandUrl( "query", query, db, precision.GetQueryParameter() ), true ).ConfigureAwait( false );
         return await QueryTransform.ParseQueryAsync<TInfluxRow>( this, queryResult, db, false ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query, string db, bool isMeasurementQuery = false )
         where TInfluxRow : IInfluxRow, new()
      {
         var queryResult = await GetInternalAsync( CreateQueryOrCommandUrl( "query", query, db ), isMeasurementQuery ).ConfigureAwait( false );
         return await QueryTransform.ParseQueryAsync<TInfluxRow>( this, queryResult, db, !isMeasurementQuery ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>( string query )
         where TInfluxRow : IInfluxRow, new()
      {
         var queryResult = await GetInternalAsync( CreateQueryOrCommandUrl( "query", query ), false ).ConfigureAwait( false );
         return await QueryTransform.ParseQueryAsync<TInfluxRow>( this, queryResult, null, false ).ConfigureAwait( false );
      }

      private async Task<InfluxResultSet> ExecuteQueryInternalAsync( string query, string db )
      {
         var queryResult = await GetInternalAsync( CreateQueryOrCommandUrl( "query", query, db ), false ).ConfigureAwait( false );
         return QueryTransform.ParseQuery( queryResult );
      }

      private async Task<InfluxResultSet> ExecuteQueryInternalAsync( string query )
      {
         var queryResult = await GetInternalAsync( CreateQueryOrCommandUrl( "query", query ), false ).ConfigureAwait( false );
         return QueryTransform.ParseQuery( queryResult );
      }

      private async Task ExecuteOperationWithNoResultAsync( string query, string db )
      {
         await GetInternalIgnoreResultAsync( CreateQueryOrCommandUrl( "query", query, db ) ).ConfigureAwait( false );
      }

      private async Task ExecuteOperationWithNoResultAsync( string query )
      {
         await GetInternalIgnoreResultAsync( CreateQueryOrCommandUrl( "query", query ) ).ConfigureAwait( false );
      }

      private async Task<QueryResult> GetInternalAsync( string url, bool isMeasurementsQuery )
      {
         try
         {
            using ( var request = new HttpRequestMessage( HttpMethod.Get, url ) )
            using ( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
               var queryResult = await response.Content.ReadAsAsync<QueryResult>().ConfigureAwait( false );
               EnsureValidQueryResult( queryResult, isMeasurementsQuery );
               return queryResult;
            }
         }
         catch ( HttpRequestException e )
         {
            throw new InfluxException( "An unknown error occurred.", e );
         }
      }

      private async Task GetInternalIgnoreResultAsync( string url )
      {
         try
         {
            using ( var request = new HttpRequestMessage( HttpMethod.Get, url ) )
            using ( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );

               // since we are ignoring the result, we dont return anything
               // but we still need to check what is being returned
               if ( response.StatusCode == HttpStatusCode.OK )
               {
                  var queryResult = await response.Content.ReadAsAsync<QueryResult>().ConfigureAwait( false );
                  EnsureValidQueryResult( queryResult, false );
               }
            }
         }
         catch ( HttpRequestException e )
         {
            throw new InfluxException( "An unknown error occurred.", e );
         }
      }

      private async Task PostInternalIgnoreResultAsync( string url, HttpContent content )
      {
         try
         {
            using ( var request = new HttpRequestMessage( HttpMethod.Post, url ) { Content = content } )
            using ( var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
            {
               await EnsureSuccessCode( response ).ConfigureAwait( false );
            }
         }
         catch ( HttpRequestException e )
         {
            throw new InfluxException( "An unknown error occurred.", e );
         }
      }

      private void EnsureValidQueryResult( QueryResult queryResult, bool isMeasurementsQuery )
      {
         // If there is only one result, we will throw an exception
         if ( queryResult.Results.Count == 1 )
         {
            var resultWrapper = queryResult.Results[ 0 ];
            if ( resultWrapper.Error != null )
            {
               throw new InfluxException( resultWrapper.Error );
            }

            if ( isMeasurementsQuery && resultWrapper.Series == null )
            {
               throw new InfluxException( Errors.UnexpectedQueryResult );
            }
         }
      }

      private async Task EnsureSuccessCode( HttpResponseMessage response )
      {
         if ( !response.IsSuccessStatusCode )
         {
            var errorResult = await response.Content.ReadAsAsync<ErrorResult>();
            if ( errorResult?.Error != null )
            {
               throw new InfluxException( errorResult.Error );
            }
            else
            {
               response.EnsureSuccessStatusCode();
            }
         }
      }
   }
}
