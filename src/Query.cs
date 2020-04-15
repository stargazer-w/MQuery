using System;
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

        public IQueryable<T> WhereFrom(IQueryable<T> queryable) => 
            WhereExpression is Expression<Func<T, bool>> w ? queryable.Where(w) : queryable;

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

                var methodInfo = typeof(Queryable).GetMethods().First(m => m.Name == methodName); // BUG: First不够稳定
                methodInfo = methodInfo.MakeGenericMethod(typeof(T), selector.ReturnType);
                queryable = (methodInfo.Invoke(null, new object?[] {queryable, selector}) as IQueryable<T>)!;

                isFirst = false;
            }

            return queryable;
        }

        public IQueryable<T> GetPageFrom(IQueryable<T> queryable)
        {
            return queryable.Skip(Paging.Skip).Take(Paging.Limit);
        }

        public IQueryable<T> GetPageFrom(IQueryable<T> queryable, out int total)
        {
            total = queryable.Count();
            return GetPageFrom(queryable);
        }

        public IQueryable<T> QueryFrom(IQueryable<T> queryable)
        {
            queryable = WhereFrom(queryable);
            queryable = Order(queryable);
            return GetPageFrom(queryable);
        }
        public IQueryable<T> QueryFrom(IQueryable<T> queryable, out int total)
        {
            queryable = WhereFrom(queryable);
            queryable = Order(queryable);
            return GetPageFrom(queryable, out total);
        }
    }

    public class Query
    {
        public Query(ParameterExpression parameterExpression)
        {
            ParameterExpression = parameterExpression;
        }

        public ParameterExpression ParameterExpression {get;}

        public LambdaExpression? WhereExpression {get; set;}

        public (QueryOrderType type, LambdaExpression selector)[]? OrderExpressions {get; set;}

        public QueryPaging Paging {get; set;} = new QueryPaging();
    }
}
