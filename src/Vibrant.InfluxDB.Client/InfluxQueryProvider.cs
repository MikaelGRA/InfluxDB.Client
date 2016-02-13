using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client.Dto;
using Vibrant.InfluxDB.Client.Linq;
using Vibrant.InfluxDB.Client.Visitors;

namespace Vibrant.InfluxDB.Client
{
   public class InfluxQueryProvider : IQueryProvider
   {
      private static MethodInfo _executeByRow;

      static InfluxQueryProvider()
      {
         _executeByRow = typeof( InfluxQueryProvider ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "ExecuteByRowType" );
      }

      private readonly InfluxClient _client;
      private string _measurementName;
      private string _db;

      public InfluxQueryProvider( InfluxClient client, string db, string measurementName )
      {
         _client = client;
         _measurementName = measurementName;
         _db = db;
      }

      public IQueryable CreateQuery( Expression expression )
      {
         Type elementType = TypeHelper.GetElementType( expression.Type );
         try
         {
            return (IQueryable)Activator.CreateInstance( typeof( InfluxQuery<> ).MakeGenericType( elementType ), new object[] { this, expression, _db, _measurementName } );
         }
         catch ( TargetInvocationException tie )
         {
            throw tie.InnerException;
         }
      }

      public IQueryable<TElement> CreateQuery<TElement>( Expression expression )
      {
         return new InfluxQuery<TElement>( this, expression );
      }

      public object Execute( Expression expression )
      {
         var elementType = TypeHelper.GetElementType( expression.Type );
         return _executeByRow.MakeGenericMethod( new[] { elementType } ).Invoke( this, new[] { expression } );
      }

      public TResult Execute<TResult>( Expression expression )
      {
         return (TResult)Execute( expression );
      }

      internal string GetQueryText( Expression expression )
      {
         return new QueryBinder().Translate( expression );
      }

      private object ExecuteByRowType<TInfluxRow>( Expression expression )
      {


         return null;
      }
   }
}
