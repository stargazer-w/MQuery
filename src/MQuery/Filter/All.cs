using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MQuery.Filter
{
    public class All : IOperator
    {
        static readonly MethodInfo _all = typeof(Enumerable).GetMethods().First(x => x.ToString() == "Boolean All[TSource](System.Collections.Generic.IEnumerable`1[TSource], System.Func`2[TSource,System.Boolean])");

        public IParameterOperation Operator { get; }

        public All(IParameterOperation @operator)
        {
            Operator = @operator;
        }

        public Expression Combine(Expression left)
        {
            if(left.Type.GetInterface("ICollection`1")?.GenericTypeArguments?[0] is not Type eleType)
                throw new ArgumentException("operator all must combine a Collection expression");

            var param = Expression.Parameter(eleType);
            // x...
            var body = Operator.Combine(param);
            // x=>x...
            var predicate = Expression.Lambda(body, param);
            // left.All(x=>x...)
            var all = _all.MakeGenericMethod(eleType);
            var callAll = Expression.Call(null, all, left, predicate);
            // left==null
            var nullGuard = Expression.Equal(left, Expression.Constant(null));
            // left==null || left.All(x=>x...)
            return Expression.OrElse(nullGuard, callAll);
        }
    }
}
