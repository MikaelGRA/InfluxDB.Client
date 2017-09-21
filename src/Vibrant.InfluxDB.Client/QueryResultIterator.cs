using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Http;
using Vibrant.InfluxDB.Client.Parsers;

namespace Vibrant.InfluxDB.Client
{
   internal class QueryResultIterator<TInfluxRow>
      where TInfluxRow : new()
   {
      private readonly JsonStreamObjectIterator _objectIterator;
      private readonly InfluxQueryOptions _options;
      private readonly InfluxClient _client;
      private readonly string _db;

      private InfluxResultSet<TInfluxRow> _currentSet;
      private int _resultIndex = 0;
      private int _serieIndex = 0;


      // current statementId
      // current "group identifier"/"name"?

      internal QueryResultIterator( JsonStreamObjectIterator objectIterator, InfluxClient client, InfluxQueryOptions options, string db )
      {
         _objectIterator = objectIterator;
         _client = client;
         _options = options;
         _db = db;
      }

      public async Task<bool> ConsumeNextAsync()
      {
         if( _currentSet == null )
         {
            _currentSet = await ResultSetFactory.CreateAsync<TInfluxRow>( _client, _objectIterator, _db, _options.Precision, true, _options.MetadataExpiration ).ConfigureAwait( false );
            _resultIndex = 0;
            _serieIndex = 0;
         }
         else
         {
            // figure out if we should increase serie/result index OR make another query result
         }

         // need to store current info
         // current statementId
         // current "group identifier"/"name"?

         return Current;
      }

      public List<TInfluxRow> Current
      {
         get
         {
            return _currentSet.Results[ _resultIndex ].Series[ _serieIndex ].Rows;
         }
      }


   }
}
