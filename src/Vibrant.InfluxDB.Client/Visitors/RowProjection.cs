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

      private Delegate GetProjector()
      {
         if( _compiledProjector == null )
         {
            Expression body = null;

            var current = this;
            var inputParameter = Expression.Parameter( current.Projector.Parameters[ 0 ].Type, "x" );
            while( current != null )
            {
               //Expression.Call( null, SelectMethod,  )

               body = Expression.Invoke( current.Projector, body ?? inputParameter );
               current = current.InnerProjection;
            }

            var outputParameterType = body.Type;

            var delegateType = typeof( Func<,> ).MakeGenericType( new Type[] { inputParameter.Type, outputParameterType } );

            var lambda = Expression.Lambda( delegateType, body, inputParameter );

            _compiledProjector = lambda.Compile();
         }
         return _compiledProjector;
      }

      public object PerformProjections( object item )
      {
         // TODO: increase performance be compiling a single function which is a combination of all

         return GetProjector().DynamicInvoke( new[] { item } );
      }
   }
}
