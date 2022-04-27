using System;

namespace MQuery.Filter
{
    public interface ICompareNode
    {
        CompareOperator Operator { get; }

        object? Value { get; }

        Type ValueType { get; }
    }

    public record CompareNode<T> : ICompareNode
    {
        public CompareOperator Operator { get; }

        public T Value { get; }

        object? ICompareNode.Value => Value;

        Type ICompareNode.ValueType => typeof(T);

        public CompareNode(CompareOperator @operator, T value) 
        {
            Operator = @operator;
            Value = value;
        }
    }
}
