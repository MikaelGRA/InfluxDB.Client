using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.Linq
{
    public static class AsyncQueryable
    {
        public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(this IQueryable<TElement> query)
        {
            var queryExpression = query.Expression;
            var asyncProvider = query.Provider as IAsyncQueryProvider;
            if (asyncProvider != null)
            {
                return asyncProvider.ExecuteAsync<IEnumerable<TElement>>(queryExpression);
            }
            else 
            {
                return Task.Run<IEnumerable<TElement>>(() => query.Provider.Execute<IEnumerable<TElement>>(queryExpression));
            }
        }

        public static Task<TResult> ExecuteAsync<TElement, TResult>(this IQueryable<TElement> query, Expression<Func<IQueryable<TElement>, TResult>> selector)
        {
            // get true query expression
            var queryExpression = ExpressionReplacer.Replace(selector.Body, selector.Parameters[0], query.Expression);

            var asyncProvider = query.Provider as IAsyncQueryProvider;
            if (asyncProvider != null)
            {
                return asyncProvider.ExecuteAsync<TResult>(queryExpression);
            }
            else
            {
                return Task.Run<TResult>(() => query.Provider.Execute<TResult>(queryExpression));
            }
        }
    }
}