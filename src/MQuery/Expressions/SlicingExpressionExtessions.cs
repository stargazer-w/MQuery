using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MQuery.Slicing;

namespace MQuery.Expressions
{
    public static class SlicingExpressionExtessions
    {
        private static readonly MethodInfo _skipInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IQueryable`1[TSource] Skip[TSource](System.Linq.IQueryable`1[TSource], Int32)");
        private static readonly MethodInfo _takeInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IQueryable`1[TSource] Take[TSource](System.Linq.IQueryable`1[TSource], Int32)");

        public static LambdaExpression ToExpression(this SlicingDocument slicing)
        {
            if(slicing is null)
                throw new ArgumentNullException(nameof(slicing));

            var qyType = typeof(IQueryable<>).MakeGenericType(slicing.ElementType);
            var qyParam = Expression.Parameter(qyType, "qy");
            Expression body = qyParam;
            if(slicing.Skip is int skip)
            {
                var skipInfo = _skipInfo.MakeGenericMethod(slicing.ElementType);
                body = Expression.Call(skipInfo, qyParam, Expression.Constant(skip));
            }
            if(slicing.Limit is int limit)
            {
                var takeInfo = _takeInfo.MakeGenericMethod(slicing.ElementType);
                body = Expression.Call(takeInfo, body, Expression.Constant(limit));
            }
            return Expression.Lambda(body, qyParam);
        }
    }
}
