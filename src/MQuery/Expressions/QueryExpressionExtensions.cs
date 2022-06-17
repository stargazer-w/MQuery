using System;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Expressions
{
    public static class QueryExpressionExtensions
    {
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression<T>(this QueryDocument<T> query)
        {
            var filterExpr = query.Filter.ToExpression();
            var sortExpr = query.Sort.ToExpression();
            var sliceExpr = query.Slice.ToExpression<T>();

            var filterThenSort = Expression.Invoke(sortExpr, Expression.Invoke(filterExpr, filterExpr.Parameters));
            var filterThenSortThenSlice = Expression.Invoke(sliceExpr, filterThenSort);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(filterThenSortThenSlice, filterExpr.Parameters);
        }
    }
}
