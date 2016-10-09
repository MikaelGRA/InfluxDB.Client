//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Threading.Tasks;
//using Vibrant.InfluxDB.Client.Dto;
//using Vibrant.InfluxDB.Client.Helpers;
//using Vibrant.InfluxDB.Client.Linq;
//using Vibrant.InfluxDB.Client.Visitors;

//namespace Vibrant.InfluxDB.Client
//{
//   public class InfluxQueryProvider : IQueryProvider
//   {
//      private static readonly MethodInfo ExecuteByRowMethod = typeof( InfluxQueryProvider ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "ExecuteByRowType" );
//      private static readonly MethodInfo GetQueryTextByRowTypeMethod = typeof( InfluxQueryProvider ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "GetQueryTextByRowType" );
//      private static readonly MethodInfo PerformProjectionMethod = typeof( InfluxQueryProvider ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "PerformProjection" );

//      private readonly InfluxClient _client;
//      private readonly Type _originalType;
//      private readonly string _measurementName;
//      private readonly string _db;

//      public InfluxQueryProvider( Type originalType, InfluxClient client, string db, string measurementName )
//      {
//         _originalType = originalType;
//         _client = client;
//         _measurementName = measurementName;
//         _db = db;
//      }

//      public IQueryable CreateQuery( Expression expression )
//      {
//         Type elementType = TypeHelper.GetElementType( expression.Type );
//         try
//         {
//            return (IQueryable)Activator.CreateInstance( typeof( InfluxQuery<> ).MakeGenericType( elementType ), new object[] { this, expression, _db, _measurementName } );
//         }
//         catch( TargetInvocationException tie )
//         {
//            throw tie.InnerException;
//         }
//      }

//      public IQueryable<TElement> CreateQuery<TElement>( Expression expression )
//      {
//         return new InfluxQuery<TElement>( this, expression );
//      }

//      public object Execute( Expression expression )
//      {
//         try
//         {
//            return ExecuteByRowMethod.MakeGenericMethod( new[] { _originalType } ).Invoke( this, new[] { expression } );
//         }
//         catch( TargetInvocationException tie )
//         {
//            throw tie.InnerException;
//         }
//      }

//      public TResult Execute<TResult>( Expression expression )
//      {
//         return (TResult)Execute( expression );
//      }

//      internal string GetQueryText( Expression expression )
//      {
//         try
//         {
//            return (string)GetQueryTextByRowTypeMethod.MakeGenericMethod( new[] { _originalType } ).Invoke( this, new[] { expression } );
//         }
//         catch( TargetInvocationException tie )
//         {
//            throw tie.InnerException;
//         }
//      }

//      private InfluxQueryInfo<TInfluxRow> GetQueryInfo<TInfluxRow>( Expression expression )
//         where TInfluxRow : new()
//      {
//         expression = PartialEvaluator.Eval( expression, CanBeEvaluatedLocally );
//         return new InfluxQueryInfoGenerator<TInfluxRow>().GetInfo( expression, _db, _measurementName );
//      }

//      private string GetQueryTextByRowType<TInfluxRow>( Expression expression )
//         where TInfluxRow : new()
//      {
//         return GetQueryInfo<TInfluxRow>( expression ).GenerateInfluxQL();
//      }

//      private object ExecuteByRowType<TInfluxRow>( Expression expression )
//         where TInfluxRow : new()
//      {
//         var queryInfo = GetQueryInfo<TInfluxRow>( expression );

//         var iql = queryInfo.GenerateInfluxQL();

//         // Also need async version of this
//         var result = _client.ReadAsync<TInfluxRow>( _db, iql ).GetAwaiter().GetResult();
//         var enumerable = result.Results.FirstOrDefault()?.Series.FirstOrDefault()?.Rows;

//         var projection = queryInfo.SelectClause?.Projection;
//         if( projection == null )
//         {
//            if( enumerable != null )
//            {
//               return enumerable;
//            }
//            else
//            {
//               return new List<TInfluxRow>();
//            }
//         }
//         else
//         {
//            // Use alternative Read method using the provided projection

//            var returnElementType = TypeHelper.GetElementType( expression.Type );
//            return PerformProjectionMethod.MakeGenericMethod( new[] { returnElementType } ).Invoke( this, new object[] { projection, enumerable } );
//         }
//      }

//      private List<TProjectedInfluxRow> PerformProjection<TProjectedInfluxRow>( RowProjection projection, IEnumerable<object> rows )
//      {
//         List<TProjectedInfluxRow> projectedRows = null;
//         if( rows != null )
//         {
//            projectedRows = projection.PerformProjections<TProjectedInfluxRow>( rows ).ToList();
//         }
//         else
//         {
//            projectedRows = new List<TProjectedInfluxRow>();
//         }
//         return projectedRows;
//      }

//      private bool CanBeEvaluatedLocally( Expression expression )
//      {
//         if( expression is MethodCallExpression )
//         {
//            var mce = (MethodCallExpression)expression;
//            if( mce.Method.DeclaringType == typeof( InfluxFunctions ) )
//            {
//               return false;
//            }
//         }

//         return expression.NodeType != ExpressionType.Parameter;
//      }
//   }
//}
