using System;
using System.Linq.Expressions;

namespace MQuery.Expressions
{
    public static class PropertyExpressionExtenssions
    {
        /// <summary>
        /// 将属性节点组合为目标的'.'表达式
        /// </summary>
        /// <param name="propertyNode"></param>
        /// <param name="target"></param>
        /// <returns>Target.Property形式的表达式</returns>
        public static MemberExpression ToExpression(this PropertyNode propertyNode, Expression target)
        {
            if(propertyNode is null)
                throw new ArgumentNullException(nameof(propertyNode));
            if(target is null)
                throw new ArgumentNullException(nameof(target));
            var expr = Expression.Property(target, propertyNode.PropertyInfo);
            foreach(var sub in propertyNode.SubProertyInfos)
            {
                expr = Expression.Property(expr, sub);
            }
            return expr;
        }
    }
}
