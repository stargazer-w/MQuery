using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MQuery.Sort;

namespace MQuery.Expressions
{
    public static class SortExpressionExtenssions
    {
        private static readonly MethodInfo _orderByInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderBy[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");
        private static readonly MethodInfo _orderByDescInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IOrderedQueryable`1[TSource] OrderByDescending[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])");

        public static LambdaExpression ToExpression(this SortDocument sort)
        {
            if(sort is null)
                throw new ArgumentNullException(nameof(sort));
            var qyType = typeof(IQueryable<>).MakeGenericType(sort.ElementType);
            var qyParam = Expression.Parameter(qyType, "qy");
            Expression body = qyParam;
            foreach(var sortByPropertyNode in sort.SortByPropertyNodes.Reverse())
            {
                body = sortByPropertyNode.ToExpression(body, sort.ElementType);
            }
            body = Expression.TypeAs(body, qyType);
            return Expression.Lambda(body, qyParam);
        }

        public static Expression ToExpression(this SortByPropertyNode sortByPropertyNode, Expression target, Type elementType)
        {
            var param = Expression.Parameter(elementType, "ele");
            var prop = sortByPropertyNode.Property.ToExpression(param);
            var propSelector = Expression.Lambda(prop, param);
            var orderInfo = (sortByPropertyNode.Type == SortPattern.Acs ? _orderByInfo : _orderByDescInfo)
                                .MakeGenericMethod(elementType, prop.Type);
            return Expression.Call(orderInfo, target, propSelector);
        }
    }
}
