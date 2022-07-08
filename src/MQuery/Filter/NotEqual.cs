using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class NotEqual<T> : IOperator
    {
        public T? Value { get; }

        public NotEqual(T? value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.NotEqual(left, constValue);
        }
    }
}
