//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Dto;
//using Vibrant.InfluxDB.Client.Helpers;
//using Vibrant.InfluxDB.Client.IQToolkit;
//using Vibrant.InfluxDB.Client.Linq;

//namespace Vibrant.InfluxDB.Client
//{
//   public class InfluxQueryProvider<TInfluxRow> : QueryProvider
//   {
//      private readonly InfluxClient _client;
//      private readonly Type _queryType;
//      private readonly string _db;
//      private readonly string _seriesId;

//      public InfluxQueryProvider( InfluxClient client, Type queryType, string db, string seriesId )
//      {
//         _client = client;
//         _db = db;
//         _seriesId = seriesId;
//         _queryType = queryType;
//      }

//      public IQueryable CreateQuery( Expression expression )
//      {
//         Type elementType = TypeHelper.GetElementType( expression.Type );
//         try
//         {
//            return (IQueryable)Activator.CreateInstance( typeof( InfluxQuery<> ).MakeGenericType( elementType ), new object[] { this, expression } );
//         }
//         catch ( System.Reflection.TargetInvocationException tie )
//         {
//            throw tie.InnerException;
//         }
//      }

//      private TranslateResult Translate( Expression expression )
//      {
//         expression = PartialEvaluator.Eval( expression );
//         ProjectionExpression proj = (ProjectionExpression)new QueryBinder().Bind( expression );
//         string commandText = new QueryFormatter().Format( proj.Source );
//         LambdaExpression projector = new ProjectionBuilder().Build( proj.Projector );
//         return new TranslateResult { CommandText = commandText, Projector = projector };

//         return null;
//      }

//      public IQueryable<TElement> CreateQuery<TElement>( Expression expression )
//      {
//         return new InfluxQuery<TElement>( this, expression );
//      }

//      public TResult Execute<TResult>( Expression expression )
//      {
//         return (TResult)Execute( expression );
//      }

//      public override object Execute( Expression expression )
//      {
//         //var query = Translate( expression );
//         //var projector = query.Projector.Compile();
//         ////var queryResult = _client.QueryAsync( _db, query.CommandText ).Result;
//         //var queryResult = _client.InternalQueryAsync( _db, "SELECT * FROM cpu" ).Result;

//         //SeriesResult seriesResult = null;
//         //foreach ( var result in queryResult.Results )
//         //{
//         //   foreach ( var series in result.Series )
//         //   {
//         //      if ( series.Name == _seriesId )
//         //      {
//         //         seriesResult = series;
//         //         break;
//         //      }
//         //   }
//         //}

//         //if ( seriesResult == null )
//         //{
//         //   throw new InvalidOperationException();
//         //}

//         // TODO: queryResult into queryType

//         // which type to return?

//         // TODO: transform into grouped type using delegate retrieved from expression

//         return null;

//         //Type elementType = TypeHelper.GetElementType( expression.Type );
//         //return Activator.CreateInstance(
//         //    typeof( ProjectionReader<> ).MakeGenericType( elementType ),
//         //    BindingFlags.Instance | BindingFlags.NonPublic,
//         //    null,
//         //    new object[] { seriesResult, projector },
//         //    null
//         //    );
//      }

//      public override string GetQueryText( Expression expression )
//      {
//         return Translate( expression ).CommandText;
//      }

//      // TODO: override async versions
//   }
//}
