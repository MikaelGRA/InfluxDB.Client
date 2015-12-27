using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Helpers
{
   internal class PropertyExpressionInfo<TInfluxRow>
   {
      internal readonly Action<TInfluxRow, object> SetValue;
      internal readonly Func<TInfluxRow, object> GetValue;
      internal readonly Type Type;

      internal PropertyExpressionInfo( PropertyInfo property )
      {
         // Instance type of target entity class 
         ParameterExpression instanceParam = Expression.Parameter( typeof( TInfluxRow ), "x" );

         // Instance type of target entity class 
         ParameterExpression valueParam = Expression.Parameter( typeof( object ), "y" );

         // instance.Property 
         MemberExpression getProperty = Expression.Property( instanceParam, property );

         // instance.Property = row[property] 
         BinaryExpression assignProperty = Expression.Assign( getProperty, Expression.Convert( valueParam, property.PropertyType ) );

         var getterLambda = Expression.Lambda<Func<TInfluxRow, object>>( Expression.Convert( getProperty, typeof( object ) ), true, instanceParam );
         GetValue = getterLambda.Compile();

         var setterLambda = Expression.Lambda<Action<TInfluxRow, object>>( assignProperty, true, instanceParam, valueParam );
         SetValue = setterLambda.Compile();

         var type = property.PropertyType;
         if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) )
         {
            // unwrap the nullable type
            Type = type.GetGenericArguments()[ 0 ];
         }
         else
         {
            Type = type;
         }
      }
   }
}
