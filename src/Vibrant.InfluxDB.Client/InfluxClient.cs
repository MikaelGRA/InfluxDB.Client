using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Metadata;
using Vibrant.InfluxDB.Client.Parsers;
using Vibrant.InfluxDB.Client.Resources;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Helpers;

namespace Vibrant.InfluxDB.Client
{
    /// <summary>
    /// An InfluxClient exposes all HTTP operations on InfluxDB.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="Vibrant.InfluxDB.Client.IInfluxClient" />
    public sealed class InfluxClient : IDisposable, IInfluxClient
    {
        /// <summary>
        /// The series meta cache
        /// </summary>
        private readonly Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo> _seriesMetaCache;
        /// <summary>
        /// The authz header
        /// </summary>
        private readonly AuthenticationHeaderValue _authzHeader;
        /// <summary>
        /// The client
        /// </summary>
        private readonly HttpClient _client;
        /// <summary>
        /// The endpoint
        /// </summary>
        private readonly Uri _endpoint;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;
        /// <summary>
        /// The dispose HTTP client handler
        /// </summary>
        private readonly bool _disposeHttpClientHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfluxClient"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="client">The client.</param>
        /// <param name="disposeHttpClient">if set to <c>true</c> [dispose HTTP client].</param>
        private InfluxClient(Uri endpoint, string username, string password, HttpClient client, bool disposeHttpClient)
        {
            _disposeHttpClientHandler = disposeHttpClient;
            _seriesMetaCache = new Dictionary<DatabaseMeasurementInfoKey, DatabaseMeasurementInfo>();
            _endpoint = endpoint;
            _client = client;

            DefaultWriteOptions = new InfluxWriteOptions();
            DefaultQueryOptions = new InfluxQueryOptions();
            TimestampParserRegistry = new DefaultTimestampParserRegistry();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var encoding = Encoding.GetEncoding("ISO-8859-1");
                var credentials = username + ":" + password;
                var encodedCredentialBytes = encoding.GetBytes(credentials);
                var encodedCredentials = Convert.ToBase64String(encodedCredentialBytes);
                _authzHeader = new AuthenticationHeaderValue("Basic", encodedCredentials);
            }
        }

        /// <summary>
        /// Constructs an InfluxClient that uses the specified credentials and HttpClient.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="client">The client.</param>
        public InfluxClient(Uri endpoint, string username, string password, HttpClient client)
           : this(endpoint, username, password, client, false)
        {

        }

        /// <summary>
        /// Constructs an InfluxClient that uses the specified credentials.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public InfluxClient(Uri endpoint, string username, string password)
           : this(endpoint, username, password, new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }, true), true)
        {
        }

        /// <summary>
        /// Constructs an InfluxClient that does not use any credentials.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        public InfluxClient(Uri endpoint)
           : this(endpoint, null, null)
        {

        }

        /// <summary>
        /// Gets or sets the timeout for all requests made.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
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
        /// <value>
        /// The default write options.
        /// </value>
        public InfluxWriteOptions DefaultWriteOptions { get; private set; }

        /// <summary>
        /// Gets the default query optionns.
        /// </summary>
        /// <value>
        /// The default query options.
        /// </value>
        public InfluxQueryOptions DefaultQueryOptions { get; private set; }

        /// <summary>
        /// Gets the timestamp parser registry.
        /// </summary>
        /// <value>
        /// The timestamp parser registry.
        /// </value>
        public ITimestampParserRegistry TimestampParserRegistry { get; private set; }

        #region Raw Operations

        /// <summary>
        /// Executes an arbitrary command that returns a table as a result.
        /// </summary>
        /// <typeparam name="TInfluxRow">The type of the influx row.</typeparam>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<InfluxResultSet<TInfluxRow>> ExecuteOperationAsync<TInfluxRow>(string commandOrQuery, string db, object parameters, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            return ExecuteQueryInternalAsync<TInfluxRow>(commandOrQuery, db, false, true, parameters, DefaultQueryOptions, cancellationToken);
        }

        /// <summary>
        /// Executes an arbitrary command that does not return a table.
        /// </summary>
        /// <param name="commandOrQuery">The command or query.</param>
        /// <param name="db">The database.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<InfluxResultSet> ExecuteOperationAsync(string commandOrQuery, string db, object parameters, CancellationToken cancellationToken = default)
        {
            return ExecuteQueryInternalAsync(commandOrQuery, db, true, parameters, DefaultQueryOptions, cancellationToken);
        }

        #endregion

        #region Ping

        /// <summary>
        /// Executes a ping and waits for the leader to respond.
        /// </summary>
        /// <param name="secondsToWaitForLeader">The seconds to wait for leader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<InfluxPingResult> PingAsync(int? secondsToWaitForLeader, CancellationToken cancellationToken = default)
        {
            return HeadInternalAsync(CreatePingUrl(secondsToWaitForLeader), cancellationToken);
        }

        #endregion

        #region Data Management

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
        public Task WriteAsync<TInfluxRow>(string db, string measurementName, IEnumerable<TInfluxRow> rows, InfluxWriteOptions options, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            List<HttpContent> contents = new List<HttpContent>();
            foreach (var groupOfRows in rows.GroupBy(x => x.GetType()))
            {
                var info = MetadataCache.GetOrCreate(groupOfRows.Key);

                var c = info.CreateHttpContentFor(this, groupOfRows, measurementName, options);
                contents.Add(c);
            }

            if (contents.Count == 0) return TaskHelpers.CompletedTask;
            var content = contents.Count == 1 ? contents[0] : new MultiContent(contents);


            if (options.UseGzip)
            {
                content = new GzipContent(content);
            }
            return PostInternalIgnoreResultAsync(CreateWriteUrl(db, options), content, cancellationToken);
        }

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
        public Task<InfluxResultSet<TInfluxRow>> ReadAsync<TInfluxRow>(string db, string query, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            return ExecuteQueryInternalAsync<TInfluxRow>(query, db, true, false, parameters, options, cancellationToken);
        }

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
        public Task<InfluxChunkedResultSet<TInfluxRow>> ReadChunkedAsync<TInfluxRow>(string db, string query, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            return ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>(query, db, true, false, parameters, options, cancellationToken);
        }

        /// <summary>
        /// Deletes data in accordance with the specified query
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="deleteQuery">The delete query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<InfluxResult> DeleteAsync(string db, string deleteQuery, object parameters, CancellationToken cancellationToken = default)
        {
            var resultSet = await ExecuteQueryInternalAsync(deleteQuery, db, true, parameters, DefaultQueryOptions, cancellationToken).ConfigureAwait(false);
            return resultSet.Results.FirstOrDefault();
        }

        #endregion

        internal async Task<DatabaseMeasurementInfo> GetMetaInformationAsync(string db, string measurementName, TimeSpan? expiration, CancellationToken cancellationToken = default)
        {
            var key = new DatabaseMeasurementInfoKey(db, measurementName);
            DatabaseMeasurementInfo info;

            lock (_seriesMetaCache)
            {
                _seriesMetaCache.TryGetValue(key, out info);
            }

            var now = DateTime.UtcNow;
            if (info != null)
            {
                if (!expiration.HasValue) // info never expires
                {
                    return info;
                }

                if (now - info.Timestamp < expiration.Value) // has not expired
                {
                    return info;
                }
            }

            // has expired or never existed, lets retrieve it

            // get metadata information from the store
            var tagsResult = await this.ShowTagKeysAsync(db, measurementName, cancellationToken).ConfigureAwait(false);
            var tags = tagsResult.Series.FirstOrDefault()?.Rows;

            info = new DatabaseMeasurementInfo(now);
            if (tags != null)
            {
                foreach (var row in tags)
                {
                    info.Tags.Add(row.TagKey);
                }
            }

            lock (_seriesMetaCache)
            {
                _seriesMetaCache[key] = info;
            }

            return info;
        }

        private string CreateWriteUrl(string db, InfluxWriteOptions options)
        {
            var url = $"write?db={UriHelper.SafeEscapeDataString(db)}&precision={options.Precision.GetQueryParameter()}&consistency={options.Consistency.GetQueryParameter()}";
            if (!string.IsNullOrEmpty(options.RetentionPolicy)) url += $"&rp={options.RetentionPolicy}";
            return new Uri(_endpoint, url).ToString();
        }

        private LongFormUrlEncodedContent CreateQueryPostContent(string commandOrQuery, string db, bool isTimeSeriesQuery, bool requireChunking, object parameters, InfluxQueryOptions options)
        {
            List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>(5);

            if (!string.IsNullOrEmpty(db))
            {
                param.Add(new KeyValuePair<string, string>("db", db));
            }
            if (!string.IsNullOrEmpty(commandOrQuery))
            {
                param.Add(new KeyValuePair<string, string>("q", commandOrQuery));
            }
            if (parameters != null)
            {
                param.Add(new KeyValuePair<string, string>("params", ParamsConverter.GetParams(parameters)));
            }

            if (options != null)
            {
                if (options.Precision.HasValue && isTimeSeriesQuery)
                {
                    param.Add(new KeyValuePair<string, string>("epoch", options.Precision.Value.GetQueryParameter()));
                }
                if (options.ChunkSize.HasValue)
                {
                    param.Add(new KeyValuePair<string, string>("chunked", "true"));
                    param.Add(new KeyValuePair<string, string>("chunk_size", options.ChunkSize.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }

            // add chunking if it is not already set
            if (requireChunking && options?.ChunkSize.HasValue != true)
            {
                param.Add(new KeyValuePair<string, string>("chunked", "true"));
            }

            return new LongFormUrlEncodedContent(param);
        }

        private string CreateQueryUrl(string commandOrQuery, string db, bool isTimeSeriesQuery, bool requireChunking, object parameters, InfluxQueryOptions options)
        {
            var query = "query";
            char seperator = '?';

            if (!string.IsNullOrEmpty(db))
            {
                query += $"{seperator}db={UriHelper.SafeEscapeDataString(db)}";
                seperator = '&';
            }

            if (!string.IsNullOrEmpty(commandOrQuery))
            {
                query += $"{seperator}q={UriHelper.SafeEscapeDataString(commandOrQuery)}";
                seperator = '&';
            }
            if (parameters != null)
            {
                query += $"{seperator}params={UriHelper.SafeEscapeDataString(ParamsConverter.GetParams(parameters))}";
                seperator = '&';
            }

            if (options != null)
            {
                if (options.Precision.HasValue && isTimeSeriesQuery)
                {
                    query += $"{seperator}epoch={options.Precision.Value.GetQueryParameter()}";
                    seperator = '&';
                }

                if (options.ChunkSize.HasValue)
                {
                    query += $"{seperator}chunked=true&chunk_size={options.ChunkSize.Value}";
                    seperator = '&';
                }
            }

            // add chunking if it is not already set
            if (requireChunking && options?.ChunkSize.HasValue != true)
            {
                query += $"{seperator}chunked=true";
            }

            return new Uri(_endpoint, query).ToString();
        }

        private string CreatePingUrl(int? secondsToWaitForLeader)
        {
            if (secondsToWaitForLeader.HasValue)
            {
                return new Uri(_endpoint, $"ping?wait_for_leader={secondsToWaitForLeader.Value}s").ToString();
            }
            else
            {
                return new Uri(_endpoint, "ping").ToString();
            }
        }

        private async Task<InfluxChunkedResultSet<TInfluxRow>> ExecuteQueryByObjectIteratorInternalAsync<TInfluxRow>(string query, string db, bool isTimeSeriesQuery, bool forcePost, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            var iterator = await PerformQueryInternal<TInfluxRow>(query, db, forcePost, isTimeSeriesQuery, true, parameters, options, cancellationToken).ConfigureAwait(false);
            return new InfluxChunkedResultSet<TInfluxRow>(iterator, this, options, db);
        }

        private async Task<InfluxResultSet<TInfluxRow>> ExecuteQueryInternalAsync<TInfluxRow>(string query, string db, bool isTimeSeriesQuery, bool forcePost, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            List<QueryResult> queryResults = await PerformQueryInternal(query, db, forcePost, isTimeSeriesQuery, false, parameters, options, cancellationToken).ConfigureAwait(false);
            return await ResultSetFactory.CreateAsync<TInfluxRow>(this, queryResults, db, isTimeSeriesQuery, options, cancellationToken).ConfigureAwait(false);
        }

        private async Task<InfluxResultSet> ExecuteQueryInternalAsync(string query, string db, bool forcePost, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
        {
            List<QueryResult> queryResults = await PerformQueryInternal(query, db, forcePost, false, false, parameters, options, cancellationToken).ConfigureAwait(false);
            return ResultSetFactory.Create(queryResults);
        }

        private async Task<ContextualQueryResultIterator<TInfluxRow>> PerformQueryInternal<TInfluxRow>(string query, string db, bool forcePost, bool isTimeSeriesQuery, bool requireChunking, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken)
           where TInfluxRow : new()
        {
            ContextualQueryResultIterator<TInfluxRow> iterator;
            if (options.UsePost)
            {
                iterator = await ExecuteHttpAsync<TInfluxRow>(
                    HttpMethod.Post, 
                    new Uri(_endpoint, "query").ToString(), 
                    db, options, 
                    CreateQueryPostContent(query, db, isTimeSeriesQuery, requireChunking, parameters, options), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (forcePost)
                {
                    iterator = await ExecuteHttpAsync<TInfluxRow>(
                        HttpMethod.Post, 
                        CreateQueryUrl(query, db, isTimeSeriesQuery, requireChunking, parameters, options), 
                        db, options, null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    iterator = await ExecuteHttpAsync<TInfluxRow>(
                        HttpMethod.Get, 
                        CreateQueryUrl(query, db, isTimeSeriesQuery, requireChunking, parameters, options), 
                        db, options, null, cancellationToken).ConfigureAwait(false);
                }
            }

            return iterator;
        }

        private async Task<List<QueryResult>> PerformQueryInternal(string query, string db, bool forcePost, bool isTimeSeriesQuery, bool requireChunking, object parameters, InfluxQueryOptions options, CancellationToken cancellationToken = default)
        {
            List<QueryResult> queryResults;
            if (options.UsePost)
            {
                queryResults = await ExecuteHttpAsync(
                    HttpMethod.Post, 
                    new Uri(_endpoint, "query").ToString(), 
                    CreateQueryPostContent(query, db, isTimeSeriesQuery, requireChunking, parameters, options),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (forcePost)
                {
                    queryResults = await ExecuteHttpAsync(
                        HttpMethod.Post, 
                        CreateQueryUrl(query, db, isTimeSeriesQuery, requireChunking, parameters, options),
                        null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    queryResults = await ExecuteHttpAsync(
                        HttpMethod.Get, 
                        CreateQueryUrl(query, db, isTimeSeriesQuery, requireChunking, parameters, options),
                        null, cancellationToken).ConfigureAwait(false);
                }
            }

            return queryResults;
        }

        private async Task<List<QueryResult>> ExecuteHttpAsync(HttpMethod method, string url, HttpContent content = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var request = new HttpRequestMessage(method, url) { Content = (method == HttpMethod.Get ? null : (content == null ? new StringContent("") : content)) })
                {
                    request.Headers.Authorization = _authzHeader;

                    using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        await EnsureSuccessCode(response, cancellationToken).ConfigureAwait(false);
                        return await response.Content.ReadMultipleAsJsonAsync<QueryResult>(cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new InfluxException(Errors.UnknownError, e);
            }
        }

        private async Task<ContextualQueryResultIterator<TInfluxRow>> ExecuteHttpAsync<TInfluxRow>(HttpMethod method, string url, string db, InfluxQueryOptions options, HttpContent content = null, CancellationToken cancellationToken = default)
           where TInfluxRow : new()
        {
            try
            {
                using (var request = new HttpRequestMessage(method, url) { Content = (method == HttpMethod.Get ? null : (content == null ? new StringContent("") : content)) })
                {
                    request.Headers.Authorization = _authzHeader;

                    var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    await EnsureSuccessCode(response, cancellationToken).ConfigureAwait(false);

                    var objectIterator = await response.Content.GetObjectIteratorAsync(cancellationToken).ConfigureAwait(false);
                    var iterator = new QueryResultIterator<TInfluxRow>(response, objectIterator, this, options, db);
                    var contextualIterator = new ContextualQueryResultIterator<TInfluxRow>(iterator);
                    return contextualIterator;
                }
            }
            catch (HttpRequestException e)
            {
                throw new InfluxException(Errors.UnknownError, e);
            }
        }

        private async Task<InfluxPingResult> HeadInternalAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    request.Headers.Authorization = _authzHeader;

                    using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        await EnsureSuccessCode(response, cancellationToken).ConfigureAwait(false);
                        IEnumerable<string> version = null;
                        response.Headers.TryGetValues("X-Influxdb-Version", out version);
                        return new InfluxPingResult { Version = version?.FirstOrDefault() ?? "unknown" };
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new InfluxException(Errors.UnknownError, e);
            }
        }

        private async Task PostInternalIgnoreResultAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content })
                {
                    request.Headers.Authorization = _authzHeader;

                    using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        await EnsureSuccessCode(response, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                throw new InfluxException(Errors.UnknownError, e);
            }
        }

        private async Task EnsureSuccessCode(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorResult = await response.Content.ReadAsJsonAsync<ErrorResult>(cancellationToken).ConfigureAwait(false);
                    if (errorResult?.Error != null)
                    {
                        throw new InfluxException(errorResult.Error);
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (JsonSerializationException e)
                {
                    throw new InfluxException(Errors.ParsingError, e);
                }
            }
        }

        #region IDisposable

        /// <summary>
        /// Destructor.
        /// </summary>
        ~InfluxClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the InfluxClient and the internal HttpClient that it uses.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposeHttpClientHandler)
                {
                    _client.Dispose();
                }
            }
        }

        #endregion
    }
}
