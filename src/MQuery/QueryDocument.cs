using System;
using System.Linq;
using System.Linq.Expressions;
using MQuery.Filter;
using MQuery.Slice;
using MQuery.Sort;

namespace MQuery
{
    public class QueryDocument<T>
    {
        public FilterDocument<T> Filter { get; } = new();

        public SortDocument<T> Sort { get; } = new();

        public SliceDocument Slice { get; } = new();

        public Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression()
        {
            var filterExpr = Filter.ToExpression();
            var sortExpr = Sort.ToExpression();
            var sliceExpr = Slice.ToExpression<T>();

            var filterThenSort = Expression.Invoke(sortExpr, Expression.Invoke(filterExpr, filterExpr.Parameters));
            var filterThenSortThenSlice = Expression.Invoke(sliceExpr, filterThenSort);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(filterThenSortThenSlice, filterExpr.Parameters);
        }
    }
}
