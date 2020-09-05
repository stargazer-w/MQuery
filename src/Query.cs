using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MQuery
{
    [ModelBinder(BinderType = typeof(QueryBinder))]
    public class Query<T> : Query
    {
        #region 懒初始化字段
        private static readonly Lazy<MethodInfo> _whereInfo
            = new Lazy<MethodInfo>(() => Query.WhereInfo.MakeGenericMethod(typeof(T)));
        private static readonly ConcurrentDictionary<Type, MethodInfo> _orderByInfos = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> _orderByDescInfos = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly Lazy<Expression<Func<IQueryable<T>, IQueryable<T>>>> _emptyExpression
            = new Lazy<Expression<Func<IQueryable<T>, IQueryable<T>>>>(() =>
            {
                var paramExpr = Expression.Parameter(typeof(IQueryable<T>));
                return (Expression<Func<IQueryable<T>, IQueryable<T>>>)Expression.Lambda(paramExpr, paramExpr);
            });
        #endregion
        protected new static MethodInfo WhereInfo => _whereInfo.Value;
        protected MethodInfo MakeOrderByInfo(Type tKey)
            => _orderByInfos.GetOrAdd(tKey, t => OrderByInfo.MakeGenericMethod(typeof(T), tKey));
        protected MethodInfo MakeOrderByDescInfo(Type tKey)
            => _orderByDescInfos.GetOrAdd(tKey, t => OrderByDescInfo.MakeGenericMethod(typeof(T), tKey));
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> EmptyExpression => _emptyExpression.Value;

        public Query() : base(Expression.Parameter(typeof(T)))
        {
        }

        public IQueryable<T> Filter(IQueryable<T> queryable) =>
            WhereExpression is Expression<Func<T, bool>> w ? queryable.Where(w) : queryable;

        public IQueryable<T> Order(IQueryable<T> queryable)
        {
            if (OrderExpressions == null)
                return queryable;

            bool isFirst = true;
            foreach (var (type, selector) in OrderExpressions)
            {
                var methodName = type switch
                {
                    QueryOrderType.Asc when isFirst  => "OrderBy",
                    QueryOrderType.Asc               => "ThenBy",
                    QueryOrderType.Desc when isFirst => "OrderByDescending",
                    QueryOrderType.Desc              => "ThenByDescending",
                    _                                => throw new Exception(),
                };

                var methodInfo = typeof(Queryable).GetMethods()
                                                  .First(m =>
                                                             m.Name == methodName && m.GetParameters().Length == 2
                                                  ); // BUG: First不够稳定
                methodInfo = methodInfo.MakeGenericMethod(typeof(T), selector.ReturnType);
                queryable  = (methodInfo.Invoke(null, new object?[] {queryable, selector}) as IQueryable<T>)!;

                isFirst = false;
            }

            return queryable;
        }

        public IQueryable<T> Slice(IQueryable<T> queryable)
        {
            return queryable.Skip(Slicing.Skip).Take(Slicing.Limit);
        }

        public IQueryable<T> Slice(IQueryable<T> queryable, out int total)
        {
            total = queryable.Count();
            return Slice(queryable);
        }

        public IQueryable<T> QueryFrom(IQueryable<T> queryable)
        {
            queryable = Filter(queryable);
            queryable = Order(queryable);
            return Slice(queryable);
        }

        public IQueryable<T> QueryFrom(IQueryable<T> queryable, out int total)
        {
            queryable = Filter(queryable);
            queryable = Order(queryable);
            return Slice(queryable, out total);
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> BuildWherePlan(Expression<Func<IQueryable<T>, IQueryable<T>>>? baseExpression = null)
        {
            baseExpression ??= EmptyExpression;
            if(WhereExpression == null) return baseExpression;

            var callWhereExpr = Expression.Call(WhereInfo, baseExpression.Body, WhereExpression);
            return (Expression<Func<IQueryable<T>, IQueryable<T>>>)Expression.Lambda(callWhereExpr, baseExpression.Parameters);
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> BuildOrderPlan(Expression<Func<IQueryable<T>, IQueryable<T>>>? baseExpression = null)
        {
            baseExpression ??= EmptyExpression;
            if(OrderExpressions == null) return baseExpression;

            var body = baseExpression.Body;
            foreach(var (type, selector) in OrderExpressions.Reverse())
            {
                var methodInfo = type switch
                {
                    QueryOrderType.Asc => MakeOrderByInfo(selector.ReturnType),
                    QueryOrderType.Desc => MakeOrderByDescInfo(selector.ReturnType),
                    _ => throw new Exception(),
                };

                body = Expression.Call(methodInfo, body, selector);
            }
            body = Expression.Convert(body, typeof(IQueryable<T>));

            return (Expression<Func<IQueryable<T>, IQueryable<T>>>)Expression.Lambda(body, baseExpression.Parameters);
        }
    }

    public class Query
    {
        private static readonly Lazy<MethodInfo> _whereInfo = 
            new Lazy<MethodInfo>(() => typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])"));
        private static readonly Lazy<MethodInfo> _orderByInfo =
            new Lazy<MethodInfo>(() => typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderBy[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])"));
        private static readonly Lazy<MethodInfo> _orderByDescInfo =
            new Lazy<MethodInfo>(() => typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderByDescending[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])"));

        protected static MethodInfo WhereInfo => _whereInfo.Value;

        protected static MethodInfo OrderByInfo => _orderByInfo.Value;

        protected static MethodInfo OrderByDescInfo => _orderByDescInfo.Value;

        public Query(ParameterExpression parameterExpression)
        {
            ParameterExpression = parameterExpression;
        }

        public ParameterExpression ParameterExpression { get; }

        public LambdaExpression? WhereExpression { get; set; }

        public (QueryOrderType type, LambdaExpression selector)[]? OrderExpressions { get; set; }

        public QuerySlicing Slicing { get; set; } = new QuerySlicing();
    }
}
