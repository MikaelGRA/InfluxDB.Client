using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Visitors
{
   public class RowProjection
   {
      private static readonly MethodInfo SelectMethod = typeof( Enumerable ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "Select" && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2 );

      private Delegate _compiledProjector;

      public RowProjection( LambdaExpression projector, RowProjection innerProjection )
      {
         Bindings = new List<ColumnBinding>();
         InnerProjection = innerProjection;
         Projector = projector;
      }

      public RowProjection InnerProjection { get; private set; }

      public LambdaExpression Projector { get; private set; }

      public List<ColumnBinding> Bindings { get; private set; }

      private Delegate GetProjectionMethod()
      {
         if( _compiledProjector == null )
         {
            Expression body = null;

            var current = this;
            var inputParameter = Expression.Parameter( current.Projector.Parameters[ 0 ].Type, "x" );
            while( current != null )
            {
               body = Expression.Invoke( current.Projector, body ?? inputParameter );

               current = current.InnerProjection;
            }
            var outputParameterType = body.Type;
            var delegateType = typeof( Func<,> ).MakeGenericType( inputParameter.Type, outputParameterType );
            
            // replace all InfluxFunction calls
            body = InfluxFunctionReplacer.Replace( body );

            var convertSingleItemLambda = Expression.Lambda( delegateType, body, inputParameter );

            // this lambda can turn one item into another item, now we want one that does the same for an enumerable of the same
            var inputParameterEnumerableType = typeof( IEnumerable<> ).MakeGenericType( inputParameter.Type );
            var outputParameterEnumerableType = typeof( IEnumerable<> ).MakeGenericType( outputParameterType );

            var inputParameterEnumable = Expression.Parameter( inputParameterEnumerableType, "y" );

            var delegateEnumerableType = typeof( Func<,> ).MakeGenericType( inputParameterEnumerableType, outputParameterEnumerableType );

            var gsm = SelectMethod.MakeGenericMethod( inputParameter.Type, outputParameterType );

            var mce = Expression.Call( null, gsm, inputParameterEnumable, convertSingleItemLambda );

            var lambda = Expression.Lambda( delegateEnumerableType, mce, inputParameterEnumable );

            _compiledProjector = lambda.Compile();
         }
         return _compiledProjector;
      }

      public IEnumerable<TProjectedInfluxRow> PerformProjections<TProjectedInfluxRow>( IEnumerable<object> item )
      {
         return (IEnumerable<TProjectedInfluxRow>)GetProjectionMethod().DynamicInvoke( new[] { item } );
      }
   }
}
