using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MQuery.Filter;

namespace MQuery.Expressions
{
    public static class FilterExpressionExtenssions
    {
        private static readonly MethodInfo _whereInfo = typeof(Queryable).GetMethods().First(m => m.ToString() == "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])");

        /// <summary>
        /// 将筛选文档转换为筛选表达式
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>对 IQueryable 进行筛选的表达式</returns>
        public static LambdaExpression ToExpression(this FilterDocument filter)
        {
            if(filter is null)
                throw new ArgumentNullException(nameof(filter));

            var qyParam = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(filter.ElementType), "qy");
            // 空筛选不做任何处理
            if(!filter.PropertyCompares.Any())
                return Expression.Lambda(qyParam, qyParam);

            var whereInfo = _whereInfo.MakeGenericMethod(filter.ElementType); // REVIEW: 可能的性能问题
            var predicate = BuildPredicate(filter);
            var body = Expression.Call(whereInfo, qyParam, predicate);
            return Expression.Lambda(body, qyParam);
        }

        private static LambdaExpression BuildPredicate(FilterDocument filter)
        {
            var eleType = filter.ElementType;
            var param = Expression.Parameter(eleType, "ele");
            var filterExpr = filter.PropertyCompares
                                   .Select(it => it.ToExpression(param))
                                   .Aggregate(Expression.And);
            return Expression.Lambda(filterExpr, param);
        }

        /// <summary>
        /// 将属性比较节点组合为每个比较的&&聚合
        /// </summary>
        /// <param name="propertyComparesNode"></param>
        /// <param name="target">属性的目标</param>
        /// <returns>以&&聚合的比较表达式</returns>
        public static Expression ToExpression(this PropertyComparesNode propertyComparesNode, Expression target)
        {
            if(propertyComparesNode is null)
                throw new ArgumentNullException(nameof(propertyComparesNode));
            if(target is null)
                throw new ArgumentNullException(nameof(target));

            var propertySelector = propertyComparesNode.Property.ToExpression(target);
            return propertyComparesNode.Compares
                                       .Select(it => it.ToExpression(propertySelector))
                                       .Aggregate(Expression.And);
        }

        /// <summary>
        /// 将比较节点根据比较操作类型将属性与值组合为比较表达式
        /// </summary>
        /// <param name="compareNode"></param>
        /// <param name="property">需要比较的属性表达式</param>
        /// <returns>比较表达式</returns>
        public static Expression ToExpression(this in CompareNode compareNode, MemberExpression property)
        {
            if(property is null)
                throw new ArgumentNullException(nameof(property));

            var op = new Dictionary<CompareOperator, Func<Expression, Expression, Expression>>
            {
                [CompareOperator.Eq] = Expression.Equal,
                [CompareOperator.Gt] = Expression.GreaterThan,
                [CompareOperator.Gte] = Expression.GreaterThanOrEqual,
                [CompareOperator.In] = MoreExpressions.In,
                [CompareOperator.Lt] = Expression.LessThan,
                [CompareOperator.Lte] = Expression.LessThanOrEqual,
                [CompareOperator.Ne] = Expression.NotEqual,
                [CompareOperator.Nin] = (left, right) => Expression.Not(MoreExpressions.In(left, right))
            }[compareNode.Operator];

            var val = Expression.Constant(compareNode.Value);
            return op(property, val);
        }
    }
}
