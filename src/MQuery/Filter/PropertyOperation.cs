﻿using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class PropertyOperation<T> : IParameterOperation
    {
        public IPropertySelector<T> Selector { get; }

        public IOperator Operator { get; }

        public PropertyOperation(IPropertySelector<T> selector, IOperator @operator)
        {
            Selector = selector;
            Operator = @operator;
        }

        public Expression Combine(ParameterExpression left)
        {
            return Operator.Combine(Selector.ToExpression(left));
        }

        public static PropertyOperation<T> Self(IOperator @operator)
        {
            return new PropertyOperation<T>(new PropertySelector<T>(), @operator);
        }
    }
}
