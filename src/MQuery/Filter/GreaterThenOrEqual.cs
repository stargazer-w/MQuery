using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class GreaterThenOrEqual<T> : IOperator
    {
        public T? Value { get; }

        public GreaterThenOrEqual(T? value)
        {
            Value = value;
        }

        public BinaryExpression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.GreaterThanOrEqual(left, constValue);
        }

        Expression IOperator.Combine(Expression left) => Combine(left);
    }
}
