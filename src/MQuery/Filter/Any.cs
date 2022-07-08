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
                throw new ArgumentException("operator any must combine a Collection expression");

            var param = Expression.Parameter(eleType);
            // x...
            var body = Operator.Combine(param);
            // x=>x...
            var predicate = Expression.Lambda(body, param);
            // left.Any(x=>x...)
            var any = _any.MakeGenericMethod(eleType);
            var callAny = Expression.Call(null, any, left, predicate);
            // left!=null
            var nullGuard = Expression.NotEqual(left, Expression.Constant(null));
            // left!=null && left.Any(x=>x...)
            return Expression.AndAlso(nullGuard, callAny);
        }
    }
}
