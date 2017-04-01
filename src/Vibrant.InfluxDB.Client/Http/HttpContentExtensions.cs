using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vibrant.InfluxDB.Client.Http
{
   internal static class HttpContentExtensions
   {
      private static readonly JsonSerializer Serializer;

      static HttpContentExtensions()
      {
         var settings = new JsonSerializerSettings();
         settings.Converters.Add( new StringEnumConverter() );
         settings.DateParseHandling = DateParseHandling.None;


         Serializer = JsonSerializer.CreateDefault( settings );
      }

      internal static Task<T> ReadAsJsonAsync<T>( this HttpContent content, CancellationToken cancellationToken = default( CancellationToken ) )
      {
         if( content == null )
            throw new ArgumentNullException( nameof( content ) );

         return ReadAsJsonAsyncCore<T>( content, cancellationToken );
      }

      internal static Task<List<T>> ReadMultipleAsJsonAsync<T>( this HttpContent content, CancellationToken cancellationToken = default( CancellationToken ) )
      {
         if( content == null )
            throw new ArgumentNullException( nameof( content ) );

         return ReadMultipleAsJsonAsyncCore<T>( content, cancellationToken );
      }

      private async static Task<T> ReadAsJsonAsyncCore<T>( HttpContent content, CancellationToken cancellationToken )
      {
         cancellationToken.ThrowIfCancellationRequested();
         var readStream = await content.ReadAsStreamAsync().ConfigureAwait( false );
         var reader = new JsonTextReader( new StreamReader( readStream, Encoding.UTF8 ) );
         T result = Serializer.Deserialize<T>( reader );
         return result;
      }

      private async static Task<List<T>> ReadMultipleAsJsonAsyncCore<T>( HttpContent content, CancellationToken cancellationToken )
      {
         cancellationToken.ThrowIfCancellationRequested();
         var readOnlyStream = await content.ReadAsStreamAsync().ConfigureAwait( false );

         List<T> results = new List<T>();
         var reader = new JsonTextReader( new StreamReader( readOnlyStream, Encoding.UTF8 ) );
         reader.SupportMultipleContent = true;
         while( true )
         {
            if( !reader.Read() )
            {
               break;
            }

            T result = Serializer.Deserialize<T>( reader );
            results.Add( result );
         }

         return results;
      }
   }
}
