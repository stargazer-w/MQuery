using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Sort
{
    public class SortDocument<T>
    {

        public List<SortBy<T>> SortBys { get; } = new();

        public IQueryable<T> ApplyTo(IQueryable<T> source)
        {
            return ToExpression().Compile()(source);
        }

        public Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression()
        {
            Expression<Func<IQueryable<T>, IQueryable<T>>> expr = source => source;
            foreach(var sortBy in Enumerable.Reverse(SortBys))
            {
                expr = expr.Update(sortBy.Combine(expr.Body), expr.Parameters);
            }
            return expr;
        }
    }
}
