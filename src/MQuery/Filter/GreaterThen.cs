using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class GreaterThen<T> : IOperator
    {
        public T? Value { get; }

        public GreaterThen(T? value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.GreaterThan(left, constValue);
        }
    }
}
