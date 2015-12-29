// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Vibrant.InfluxDB.Client.IQToolkit
{
    public static class AsyncUpdatable
    {
        private static Task<TResult> DoAsync<TResult>(IUpdatable updatable, Expression<Func<IUpdatable, TResult>> updateFunction)
        {
            // get true query expression
            var expression = ExpressionReplacer.Replace(updateFunction.Body, updateFunction.Parameters[0], updatable.Expression);

            var asyncProvider = updatable.Provider as IAsyncQueryProvider;
            if (asyncProvider != null)
            {
                return asyncProvider.ExecuteAsync<TResult>(expression);
            }
            else
            {
                return Task.Run<TResult>(() => updatable.Provider.Execute<TResult>(expression));
            }
        }

        private static Task<TResult> DoAsync<TElement, TResult>(IUpdatable<TElement> updatable, Expression<Func<IUpdatable<TElement>, TResult>> updateFunction)
        {
            // get true query expression
            var expression = ExpressionReplacer.Replace(updateFunction.Body, updateFunction.Parameters[0], updatable.Expression);

            var asyncProvider = updatable.Provider as IAsyncQueryProvider;
            if (asyncProvider != null)
            {
                return asyncProvider.ExecuteAsync<TResult>(expression);
            }
            else
            {
                return Task.Run<TResult>(() => updatable.Provider.Execute<TResult>(expression));
            }
        }

        public static Task<object> InsertAsync(IUpdatable collection, object instance, LambdaExpression resultSelector)
        {
            return DoAsync(collection, x => Updatable.Insert(x, instance, resultSelector));
        }

        /// <summary>
        /// Insert a copy of the instance into an updatable collection.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert.</param>
        /// <returns>The value 1 if the insert succeeds, otherwise 0.</returns>
        public static Task<int> InsertAsync<T>(this IUpdatable<T> collection, T instance)
        {
            return DoAsync(collection, x => x.Insert(instance));
        }

        /// <summary>
        /// Insert an copy of the instance into the updatable collection and produce a result if the insert succeeds.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="S">The type of the result.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="resultSelector">The function that produces the result.</param>
        /// <returns>The value of the result if the insert succeed, otherwise null.</returns>
        public static Task<S> InsertAsync<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, S>> resultSelector)
        {
            return DoAsync(collection, x => x.Insert(instance, resultSelector));
        }

        public static Task<object> UpdateAsync(IUpdatable collection, object instance, LambdaExpression updateCheck, LambdaExpression resultSelector)
        {
            return DoAsync(collection, x => Updatable.Update(x, instance, updateCheck, resultSelector));
        }

        /// <summary>
        /// Update the object in the updatable collection with the values in this instance only if the update check passes and produce
        /// a result based on the updated object if the update succeeds.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="S">The type of the result.</typeparam>
        /// <param name="collection">The updatable collection</param>
        /// <param name="instance">The instance to update.</param>
        /// <param name="updateCheck">A predicate testing the suitability of the object in the collection (often used that make sure assumptions have not changed.)</param>
        /// <param name="resultSelector">A function that produces a result based on the object in the collection after the update succeeds.</param>
        /// <returns>The value of the result function if the update succeeds, otherwise null.</returns>
        public static Task<S> UpdateAsync<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck, Expression<Func<T, S>> resultSelector)
        {
            return DoAsync(collection, x => x.Update(instance, updateCheck, resultSelector));
        }

        /// <summary>
        /// Update the object in the updatable collection with the values in this instance only if the update check passes.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to update.</param>
        /// <param name="updateCheck">A predicate testing the suitability of the object in the collection.</param>
        /// <returns>The value 1 if the update succeeds, otherwise 0.</returns>
        public static Task<int> UpdateAsync<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck)
        {
            return DoAsync(collection, x => x.Update(instance, updateCheck));
        }

        /// <summary>
        /// Update the object in the updatable collection with the values in this instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to update.</param>
        /// <returns>The value 1 if the update succeeds, otherwise 0.</returns>
        public static Task<int> UpdateAsync<T>(this IUpdatable<T> collection, T instance)
        {
            return DoAsync(collection, x => x.Update(instance));
        }

        public static Task<object> InsertOrUpdateAsync(IUpdatable collection, object instance, LambdaExpression updateCheck, LambdaExpression resultSelector)
        {
            return DoAsync(collection, x => Updatable.InsertOrUpdate(x, instance, updateCheck, resultSelector));
        }

        /// <summary>
        /// Insert a copy of the instance if it does not exist in the collection or update the object in the collection with the values in this instance. 
        /// Produce a result based on the object in the collection after the insert or update succeeds.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <typeparam name="S">The type of the result.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert or update.</param>
        /// <param name="updateCheck">A predicate testing the suitablilty of the object in the collection if an update is required.</param>
        /// <param name="resultSelector">A function producing a result based on the object in the collection after the insert or update succeeds.</param>
        /// <returns>The value of the result if the insert or update succeeds, otherwise null.</returns>
        public static Task<S> InsertOrUpdateAsync<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck, Expression<Func<T, S>> resultSelector)
        {
            return DoAsync(collection, x => x.InsertOrUpdate(instance, updateCheck, resultSelector));
        }

        /// <summary>
        /// Insert a copy of the instance if it does not exist in the collection or update the object in the collection with the values in this instance. 
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert or update.</param>
        /// <param name="updateCheck">A function producing a result based on the object in the collection after the insert or update succeeds.</param>
        /// <returns>The value 1 if the insert or update succeeds, otherwise 0.</returns>
        public static Task<int> InsertOrUpdateAsync<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck)
        {
            return DoAsync(collection, x => x.InsertOrUpdate(instance, updateCheck));
        }

        /// <summary>
        /// Insert a copy of the instance if it does not exist in the collection or update the object in the collection with the values in this instance. 
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert or update.</param>
        /// <returns>The value 1 if the insert or update succeeds, otherwise 0.</returns>
        public static Task<int> InsertOrUpdateAsync<T>(this IUpdatable<T> collection, T instance)
        {
            return DoAsync(collection, x => x.InsertOrUpdate(instance));
        }

        public static Task<object> DeleteAsync(IUpdatable collection, object instance, LambdaExpression deleteCheck)
        {
            return DoAsync(collection, x => Updatable.Delete(x, instance, deleteCheck));
        }

        /// <summary>
        /// Delete the object in the collection that matches the instance only if the delete check passes.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to delete.</param>
        /// <param name="deleteCheck">A predicate testing the suitability of the corresponding object in the collection.</param>
        /// <returns>The value 1 if the delete succeeds, otherwise 0.</returns>
        public static Task<int> DeleteAsync<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> deleteCheck)
        {
            return DoAsync(collection, x => x.Delete(instance, deleteCheck));
        }

        /// <summary>
        /// Delete the object in the collection that matches the instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to delete.</param>
        /// <returns>The value 1 if the Delete succeeds, otherwise 0.</returns>
        public static Task<int> DeleteAsync<T>(this IUpdatable<T> collection, T instance)
        {
            return DoAsync(collection, x => x.Delete(instance));
        }

        public static Task<int> DeleteAsync(IUpdatable collection, LambdaExpression predicate)
        {
            return DoAsync(collection, x => Updatable.Delete(x, predicate));
        }

        /// <summary>
        /// Delete all the objects in the collection that match the predicate.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The number of objects deleted.</returns>
        public static Task<int> DeleteAsync<T>(this IUpdatable<T> collection, Expression<Func<T, bool>> predicate)
        {
            return DoAsync(collection, x => x.Delete(predicate));
        }

        public static Task<IEnumerable> BatchAsync(IUpdatable collection, IEnumerable items, LambdaExpression fnOperation, int batchSize, bool stream)
        {
            return DoAsync(collection, x => Updatable.Batch(x, items, fnOperation, batchSize, stream));
        }

        /// <summary>
        /// Apply an Insert, Update, InsertOrUpdate or Delete operation over a set of items and produce a set of results per invocation.
        /// </summary>
        /// <typeparam name="T">The type of the instances.</typeparam>
        /// <typeparam name="S">The type of each result</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instances">The instances to apply the operation to.</param>
        /// <param name="fnOperation">The operation to apply.</param>
        /// <param name="batchSize">The maximum size of each batch.</param>
        /// <param name="stream">If true then execution is deferred until the resulting sequence is enumerated.</param>
        /// <returns>A sequence of results cooresponding to each invocation.</returns>
        public static Task<IEnumerable<S>> BatchAsync<U, T, S>(this IUpdatable<U> collection, IEnumerable<T> instances, Expression<Func<IUpdatable<U>, T, S>> fnOperation, int batchSize, bool stream)
        {
            return DoAsync(collection, x => x.Batch(instances, fnOperation, batchSize, stream));
        }

        /// <summary>
        /// Apply an Insert, Update, InsertOrUpdate or Delete operation over a set of items and produce a set of result per invocation.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <typeparam name="S">The type of each result.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instances">The instances to apply the operation to.</param>
        /// <param name="fnOperation">The operation to apply.</param>
        /// <returns>A sequence of results corresponding to each invocation.</returns>
        public static Task<IEnumerable<S>> BatchAsync<U, T, S>(this IUpdatable<U> collection, IEnumerable<T> instances, Expression<Func<IUpdatable<U>, T, S>> fnOperation)
        {
            return DoAsync(collection, x => x.Batch(instances, fnOperation));
        }
    }
}