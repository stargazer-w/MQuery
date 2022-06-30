using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class LessThenOrEqual<T> : IOperator
    {
        public T? Value { get; }

        public LessThenOrEqual(T? value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.LessThanOrEqual(left, constValue);
        }
    }
}
