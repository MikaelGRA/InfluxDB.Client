using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client
{
   public static class InfluxQueryableExtensions
   {
      private static readonly MethodInfo GroupByTimeMethod = typeof( InfluxQueryableExtensions ).GetTypeInfo().DeclaredMethods.First( x => x.Name == "GroupByTime" );

      public static IQueryable<TSource> GroupByTime<TSource>( this IQueryable<TSource> source, TimeSpan timespan )
      {
         if( source == null )
            throw new ArgumentNullException( nameof( source ) );
         return source.Provider.CreateQuery<TSource>(
             Expression.Call(
                 null,
                 GroupByTimeMethod.MakeGenericMethod( typeof( TSource ) ),
                 new Expression[] { source.Expression, Expression.Constant( timespan ) }
                 ) );
      }
   }
}
