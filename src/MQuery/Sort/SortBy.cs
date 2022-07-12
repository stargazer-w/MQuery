using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Sort
{
    public class SortBy<T>
    {
        private static readonly MethodInfo _orderByInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderBy[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");
        private static readonly MethodInfo _orderByDescInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderByDescending[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");

        public PropertySelector<T> Selector { get; }

        public SortPattern Pattern { get; }

        public SortBy(PropertySelector<T> selector, SortPattern pattern)
        {
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
            Pattern = pattern;
        }

        public Expression Combine(Expression left)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var lambdaSelector = Expression.Lambda(Selector.ToExpression(param), param);
            var method = (Pattern switch
            {
                SortPattern.Acs => _orderByInfo,
                SortPattern.Desc => _orderByDescInfo,
                _ => throw new NotSupportedException(),
            }).MakeGenericMethod(typeof(T), Selector.PropertyType);
            return Expression.Call(null, method, left, lambdaSelector);
        }
    }
}
