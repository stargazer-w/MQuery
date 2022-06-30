using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Filter
{
    public class Any : IOperator
    {
        static readonly MethodInfo _any = typeof(Enumerable).GetMethods().First(x => x.ToString() == "Boolean Any[TSource](System.Collections.Generic.IEnumerable`1[TSource], System.Func`2[TSource,System.Boolean])");

        public IParameterOperation Operator { get; }

        public Any(IParameterOperation @operator)
        {
            Operator = @operator;
        }

        public Expression Combine(Expression left)
        {
            if(left.Type.GetInterface("ICollection`1")?.GenericTypeArguments?[0] is not Type eleType)
                throw new ArgumentException("any-operator must combine a Collection expression");

            var any = _any.MakeGenericMethod(eleType);
            var param = Expression.Parameter(eleType);
            var body = Operator.Combine(param);
            var predicate = Expression.Lambda(body, param);

            return Expression.Call(null, any, left, predicate);
        }
    }
}
