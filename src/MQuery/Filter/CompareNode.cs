namespace MQuery.Filter
{
    public record CompareNode
    {
        public CompareOperator Operator { get; }

        public object? Value { get; }

        public CompareNode(CompareOperator @operator, object? value)
        {
            Operator = @operator;
            Value = value;
        }
    }
}
