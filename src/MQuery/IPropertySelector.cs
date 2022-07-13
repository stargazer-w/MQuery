using System;
using System.Linq.Expressions;

namespace MQuery
{
    public interface IPropertySelector<T>
    {
        Type PropertyType { get; }
        /// <summary>
        /// 若属性类型是一个集合类型，则返回集合的元素类型，否则返回null
        /// </summary>
        Type? PropertyCollectionElementType { get; }

        Expression ToExpression(ParameterExpression left);
    }
}