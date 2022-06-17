using MQuery.Filter;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Expressions
{
    public static class FilterExpressionExtensions
    {
        private static readonly MethodInfo _whereInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])");
        private static readonly MethodInfo _anyInfo = typeof(Enumerable).GetMethods().First(m => m.ToString() == "Boolean Any[TSource](System.Collections.Generic.IEnumerable`1[TSource], System.Func`2[TSource,System.Boolean])");

        /// <summary>
        /// 将筛选文档转换为筛选表达式
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>对 IQueryable 进行筛选的表达式</returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> ToExpression<T>(this FilterDocument<T> filter)
        {
            if(filter is null)
                throw new ArgumentNullException(nameof(filter));

            Expression<Func<IQueryable<T>, IQueryable<T>>> noop = qy => qy;

            var whereInfo = _whereInfo.MakeGenericMethod(typeof(T));
            var body = noop.Body;
            foreach(var propCompare in filter.PropertyCompares)
            {
                var predicate = BuildWherePredicate<T>(propCompare);
                body = Expression.Call(whereInfo, body, predicate);
            }

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(body, noop.Parameters);
        }

        public static Expression<Func<T, bool>> BuildWherePredicate<T>(IPropertyComparesNode propertyComparesNode)
        {
            if(propertyComparesNode is null)
            {
                throw new ArgumentNullException(nameof(propertyComparesNode));
            }

            var selector = propertyComparesNode.PropertySelector;
            if(selector.ReturnType.GetInterface("ICollection`1") is not null)
            {
                // x => x.Prop.Any(x => x <op> value)
                var body = BuildAnyCompareBody(
                    selector.Body,
                    propertyComparesNode.Operator,
                    propertyComparesNode.Value,
                    propertyComparesNode.Type
                );

                return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
            }
            else
            {
                // x => x.Prop <op> value
                var body = BuildCompareBody(
                    selector.Body,
                    propertyComparesNode.Operator,
                    propertyComparesNode.Value,
                    propertyComparesNode.Type
                );

                return Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
            }
        }

        private static Expression BuildAnyCompareBody<TValue>(Expression left, CompareOperator @operator, TValue rightValue, Type valueType)
        {
            var eleType = left.Type.GetInterface("ICollection`1").GenericTypeArguments[0];
            var anyInfo = _anyInfo.MakeGenericMethod(eleType);
            var predicate = BuildAnyPredicate(eleType, @operator, rightValue, valueType);
            return Expression.Call(anyInfo, left, predicate);
        }

        private static LambdaExpression BuildAnyPredicate<TValue>(Type eleType, CompareOperator @operator, TValue rightValue, Type valueType)
        {
            var param = Expression.Parameter(eleType, "x");
            var body = BuildCompareBody(param, @operator, rightValue, valueType);
            return Expression.Lambda(body, param);
        }

        private static Expression BuildCompareBody(Expression left, CompareOperator @operator, object? rightValue, Type valueType)
        {
            Func<Expression, Expression, Expression> op = @operator switch
            {
                CompareOperator.Eq => Expression.Equal,
                CompareOperator.Gt => Expression.GreaterThan,
                CompareOperator.Gte => Expression.GreaterThanOrEqual,
                CompareOperator.In => MoreExpressions.In,
                CompareOperator.Lt => Expression.LessThan,
                CompareOperator.Lte => Expression.LessThanOrEqual,
                CompareOperator.Ne => Expression.NotEqual,
                CompareOperator.Nin => (left, right) => Expression.Not(MoreExpressions.In(left, right)),
                _ => throw new ArgumentOutOfRangeException("error operator", nameof(@operator)),
            };

            var right = Expression.Constant(rightValue, valueType);
            return op(left, right);
        }
    }
}
