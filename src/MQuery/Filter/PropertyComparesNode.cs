using System;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public interface IPropertyComparesNode
    {
        CompareOperator Operator { get; }
        LambdaExpression PropertySelector { get; }
        object? Value { get; }
        Type Type { get; }
    }

    public class PropertyComparesNode<TValue> : IPropertyComparesNode
    {
        public LambdaExpression PropertySelector { get; }

        public CompareOperator Operator { get; }

        public TValue Value { get; }

        object? IPropertyComparesNode.Value => Value;

        Type IPropertyComparesNode.Type => typeof(TValue);

        public PropertyComparesNode(LambdaExpression selector, CompareOperator @operator, TValue value)
        {
            if(selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            PropertySelector = selector;
            Operator = @operator;
            Value = value;
        }
    }
}
