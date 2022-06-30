using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Filter
{
    public class In<T> : IOperator
    {
        static readonly MethodInfo _contains = typeof(Enumerable).GetMethods().First(x => x.ToString() == "Boolean Contains[TSource](System.Collections.Generic.IEnumerable`1[TSource], TSource)").MakeGenericMethod(typeof(T));

        public IEnumerable<T?> Value { get; }

        public In(IEnumerable<T?> value)
        {
            Value = value;
        }

        public Expression Combine(Expression left)
        {
            var constValue = Expression.Constant(Value, typeof(IEnumerable<T>));
            return Expression.Call(null, _contains, constValue, left);
        }
    }
}
