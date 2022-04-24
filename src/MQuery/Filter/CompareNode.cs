namespace MQuery.Filter
{
    public abstract record CompareNode
    {
        public CompareOperator Operator { get; }

        public abstract object? Value { get; }

        public CompareNode(CompareOperator @operator)
        {
            Operator = @operator;
        }
    }

    public record CompareNode<T> : CompareNode
    {
        private T _value;

        public override object? Value => _value;

        public CompareNode(CompareOperator @operator, T value) : base(@operator)
        {
            _value = value;
        }
    }
}
