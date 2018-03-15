using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Helpers.QueryProvider
{
    public abstract class BaseProvider : IQueryProvider, IQueryText
    {
        protected BaseProvider()
        {
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new Query<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)this.Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.Execute(expression);
        }

        public abstract string GetQueryText(Expression expression);
        public abstract object Execute(Expression expression);
    }

    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        IQueryProvider provider;
        Expression expression;

        public Query(IQueryProvider provider)
            : this(provider, null)
        {
        }

        public Query(IQueryProvider provider, Type staticType)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }
            this.provider = provider;
            this.expression = staticType != null ? Expression.Constant(this, staticType) : Expression.Constant(this);
        }

        public Query(BaseProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("Provider");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }
            this.provider = provider;
            this.expression = expression;
        }

        public Expression Expression
        {
            get { return this.expression; }
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider
        {
            get { return this.provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }

        public override string ToString()
        {
            if (this.expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)this.expression).Value == this)
            {
                return "Query(" + typeof(T) + ")";
            }
            else
            {
                return this.expression.ToString();
            }
        }

        public string QueryText
        {
            get
            {
                IQueryText iqt = this.provider as IQueryText;
                if (iqt != null)
                {
                    return iqt.GetQueryText(this.expression);
                }
                return "";
            }
        }
    }

    public static class Updatable
    {
        public static object Insert(IUpdatable collection, object instance, LambdaExpression resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                (MethodInfo)MethodInfo.GetCurrentMethod(),
                collection.Expression,
                Expression.Constant(instance),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(LambdaExpression))
                );
            return collection.Provider.Execute(callMyself);
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
        public static S Insert<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, S>> resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(S)),
                collection.Expression,
                Expression.Constant(instance),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(Expression<Func<T,S>>))
                );
            return (S)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Insert a copy of the instance into an updatable collection.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert.</param>
        /// <returns>The value 1 if the insert succeeds, otherwise 0.</returns>
        public static int Insert<T>(this IUpdatable<T> collection, T instance)
        {
            return Insert<T, int>(collection, instance, null);
        }

        public static object Update(IUpdatable collection, object instance, LambdaExpression updateCheck, LambdaExpression resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                (MethodInfo)MethodInfo.GetCurrentMethod(),
                collection.Expression,
                Expression.Constant(instance),
                updateCheck != null ? (Expression)Expression.Quote(updateCheck) : Expression.Constant(null, typeof(LambdaExpression)),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(LambdaExpression))
                );
            return collection.Provider.Execute(callMyself);
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
        public static S Update<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck, Expression<Func<T, S>> resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(S)),
                collection.Expression,
                Expression.Constant(instance),
                updateCheck != null ? (Expression)Expression.Quote(updateCheck) : Expression.Constant(null, typeof(Expression<Func<T, bool>>)),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(Expression<Func<T, S>>))
                );
            return (S)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Update the object in the updatable collection with the values in this instance only if the update check passes.
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to update.</param>
        /// <param name="updateCheck">A predicate testing the suitability of the object in the collection.</param>
        /// <returns>The value 1 if the update succeeds, otherwise 0.</returns>
        public static int Update<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck)
        {
            return Update<T, int>(collection, instance, updateCheck, null);
        }

        /// <summary>
        /// Update the object in the updatable collection with the values in this instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to update.</param>
        /// <returns>The value 1 if the update succeeds, otherwise 0.</returns>
        public static int Update<T>(this IUpdatable<T> collection, T instance)
        {
            return Update<T, int>(collection, instance, null, null);
        }

        public static object InsertOrUpdate(IUpdatable collection, object instance, LambdaExpression updateCheck, LambdaExpression resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                (MethodInfo)MethodInfo.GetCurrentMethod(),
                collection.Expression,
                Expression.Constant(instance),
                updateCheck != null ? (Expression)Expression.Quote(updateCheck) : Expression.Constant(null, typeof(LambdaExpression)),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(LambdaExpression))
                );
            return collection.Provider.Execute(callMyself);
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
        public static S InsertOrUpdate<T, S>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck, Expression<Func<T, S>> resultSelector)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T), typeof(S)),
                collection.Expression,
                Expression.Constant(instance),
                updateCheck != null ? (Expression)Expression.Quote(updateCheck) : Expression.Constant(null, typeof(Expression<Func<T, bool>>)),
                resultSelector != null ? (Expression)Expression.Quote(resultSelector) : Expression.Constant(null, typeof(Expression<Func<T, S>>))
                );
            return (S)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Insert a copy of the instance if it does not exist in the collection or update the object in the collection with the values in this instance. 
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert or update.</param>
        /// <param name="updateCheck">A function producing a result based on the object in the collection after the insert or update succeeds.</param>
        /// <returns>The value 1 if the insert or update succeeds, otherwise 0.</returns>
        public static int InsertOrUpdate<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> updateCheck)
        {
            return InsertOrUpdate<T, int>(collection, instance, updateCheck, null);
        }

        /// <summary>
        /// Insert a copy of the instance if it does not exist in the collection or update the object in the collection with the values in this instance. 
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to insert or update.</param>
        /// <returns>The value 1 if the insert or update succeeds, otherwise 0.</returns>
        public static int InsertOrUpdate<T>(this IUpdatable<T> collection, T instance)
        {
            return InsertOrUpdate<T, int>(collection, instance, null, null);
        }

        public static object Delete(IUpdatable collection, object instance, LambdaExpression deleteCheck)
        {
            var callMyself = Expression.Call(
                null,
                (MethodInfo)MethodInfo.GetCurrentMethod(),
                collection.Expression,
                Expression.Constant(instance),
                deleteCheck != null ? (Expression)Expression.Quote(deleteCheck) : Expression.Constant(null, typeof(LambdaExpression))
                );
            return collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Delete the object in the collection that matches the instance only if the delete check passes.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to delete.</param>
        /// <param name="deleteCheck">A predicate testing the suitability of the corresponding object in the collection.</param>
        /// <returns>The value 1 if the delete succeeds, otherwise 0.</returns>
        public static int Delete<T>(this IUpdatable<T> collection, T instance, Expression<Func<T, bool>> deleteCheck)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                Expression.Constant(instance),
                deleteCheck != null ? (Expression)Expression.Quote(deleteCheck) : Expression.Constant(null, typeof(Expression<Func<T, bool>>))
                );
            return (int)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Delete the object in the collection that matches the instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instance">The instance to delete.</param>
        /// <returns>The value 1 if the Delete succeeds, otherwise 0.</returns>
        public static int Delete<T>(this IUpdatable<T> collection, T instance)
        {
            return Delete<T>(collection, instance, null);
        }

        public static int Delete(IUpdatable collection, LambdaExpression predicate)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()),
                collection.Expression,
                predicate != null ? (Expression)Expression.Quote(predicate) : Expression.Constant(null, typeof(LambdaExpression))
                );
            return (int)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Delete all the objects in the collection that match the predicate.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The number of objects deleted.</returns>
        public static int Delete<T>(this IUpdatable<T> collection, Expression<Func<T, bool>> predicate)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                collection.Expression,
                predicate != null ? (Expression)Expression.Quote(predicate) : Expression.Constant(null, typeof(Expression<Func<T, bool>>))
                );
            return (int)collection.Provider.Execute(callMyself);
        }

        public static IEnumerable Batch(IUpdatable collection, IEnumerable items, LambdaExpression fnOperation, int batchSize, bool stream)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()),
                collection.Expression,
                Expression.Constant(items),
                fnOperation != null ? (Expression)Expression.Quote(fnOperation) : Expression.Constant(null, typeof(LambdaExpression)),
                Expression.Constant(batchSize),
                Expression.Constant(stream)
                );
            return (IEnumerable)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Apply an Insert, Update, InsertOrUpdate or Delete operation over a set of items and produce a set of results per invocation.
        /// </summary>
        /// <typeparam name="U">The type of the collection.</typeparam>
        /// <typeparam name="T">The type of the instances.</typeparam>
        /// <typeparam name="S">The type of each result</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instances">The instances to apply the operation to.</param>
        /// <param name="fnOperation">The operation to apply.</param>
        /// <param name="batchSize">The maximum size of each batch.</param>
        /// <param name="stream">If true then execution is deferred until the resulting sequence is enumerated.</param>
        /// <returns>A sequence of results cooresponding to each invocation.</returns>
        public static IEnumerable<S> Batch<U,T,S>(this IUpdatable<U> collection, IEnumerable<T> instances, Expression<Func<IUpdatable<U>, T, S>> fnOperation, int batchSize, bool stream)
        {
            var callMyself = Expression.Call(
                null,
                ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(U), typeof(T), typeof(S)),
                collection.Expression,
                Expression.Constant(instances),
                fnOperation != null ? (Expression)Expression.Quote(fnOperation) : Expression.Constant(null, typeof(Expression<Func<IUpdatable<U>, T, S>>)),
                Expression.Constant(batchSize),
                Expression.Constant(stream)
                );
            return (IEnumerable<S>)collection.Provider.Execute(callMyself);
        }

        /// <summary>
        /// Apply an Insert, Update, InsertOrUpdate or Delete operation over a set of items and produce a set of result per invocation.
        /// </summary>
        /// <typeparam name="U">The type of the collection.</typeparam>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <typeparam name="S">The type of each result.</typeparam>
        /// <param name="collection">The updatable collection.</param>
        /// <param name="instances">The instances to apply the operation to.</param>
        /// <param name="fnOperation">The operation to apply.</param>
        /// <returns>A sequence of results corresponding to each invocation.</returns>
        public static IEnumerable<S> Batch<U, T, S>(this IUpdatable<U> collection, IEnumerable<T> instances, Expression<Func<IUpdatable<U>, T, S>> fnOperation)
        {
            return Batch<U, T, S>(collection, instances, fnOperation, 50, false);
        }
    }
}
