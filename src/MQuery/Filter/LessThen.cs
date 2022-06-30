using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class LessThen<T> : IOperator
    {
        public T? Value { get; }

        public LessThen(T? value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.LessThan(left, constValue);
        }
    }
}
