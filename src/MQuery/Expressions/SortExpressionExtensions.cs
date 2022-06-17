using MQuery.Sort;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Expressions
{
    public static class SortExpressionExtensions
    {
        private static readonly MethodInfo _orderByInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderBy[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");
        private static readonly MethodInfo _orderByDescInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderByDescending[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");

        public static Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression<T>(this SortDocument<T> sort)
        {
            if(sort is null)
                throw new ArgumentNullException(nameof(sort));

            var qyType = typeof(IQueryable<>).MakeGenericType(typeof(T));
            var qyParam = Expression.Parameter(qyType, "qy");
            Expression body = qyParam;
            foreach(var sortByPropertyNode in sort.SortByPropertyNodes.Reverse())
            {
                body = sortByPropertyNode.ToExpression(body, typeof(T));
            }
            body = Expression.TypeAs(body, qyType);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(body, qyParam);
        }

        public static Expression ToExpression(this SortByPropertyNode sortByPropertyNode, Expression target, Type elementType)
        {
            var prop = sortByPropertyNode.PropertySelector;
            var orderInfo = (sortByPropertyNode.Pattern == SortPattern.Acs ? _orderByInfo : _orderByDescInfo)
                                .MakeGenericMethod(elementType, prop.ReturnType);
            return Expression.Call(orderInfo, target, prop);
        }
    }
}
