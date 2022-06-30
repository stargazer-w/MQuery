using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class Equal<T> : IOperator
    {
        public T? Value { get; }

        public Equal(T? value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(T));
            return Expression.Equal(left, constValue);
        }
    }
}
