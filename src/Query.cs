using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace MQuery
{
    [ModelBinder(BinderType = typeof(QueryBinder))]
    public class Query<T> : Query
    {
        public Query() : base(Expression.Parameter(typeof(T)))
        {
        }

        public new Expression<Func<T, bool>> WhereExpression =>
            base.WhereExpression is Expression<Func<T, bool>> exp ? exp : it => true;

        public IQueryable<T> Filter(IQueryable<T> queryable) => queryable.Where(WhereExpression);

        public IQueryable<T> Order(IQueryable<T> queryable)
        {
            if(OrderExpressions == null)
                return queryable;

            bool isFirst = true;
            foreach (var (type, selector) in OrderExpressions)
            {
                var methodName = type switch
                {
                    QueryOrderType.Asc when isFirst => "OrderBy",
                    QueryOrderType.Asc => "ThenBy",
                    QueryOrderType.Desc when isFirst => "OrderByDescending",
                    QueryOrderType.Desc => "ThenByDescending",
                    _ => throw new Exception(),
                };

                var methodInfo = typeof(Queryable).GetMethods().First(m => m.Name == methodName && m.GetParameters().Length == 2); // BUG: First不够稳定
                methodInfo = methodInfo.MakeGenericMethod(typeof(T), selector.ReturnType);
                queryable = (methodInfo.Invoke(null, new object?[] { queryable, selector }) as IQueryable<T>)!;

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
    }

    public class Query
    {
        public Query(ParameterExpression parameterExpression)
        {
            ParameterExpression = parameterExpression;
        }

        internal ParameterExpression ParameterExpression { get; }

        internal LambdaExpression? WhereExpression { get; set; }

        internal (QueryOrderType type, LambdaExpression selector)[]? OrderExpressions { get; set; }

        public QuerySlicing Slicing { get; set; } = new QuerySlicing();
    }
}
