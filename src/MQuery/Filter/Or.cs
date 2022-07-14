using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class Or : IParameterOperation
    {
        public IList<IParameterOperation> Operations { get; }

        public Or(params IParameterOperation[] operations)
        {
            Operations = operations.ToList();
        }

        public Expression Combine(ParameterExpression left)
        {
            var first = Operations.FirstOrDefault();
            if(first == null)
                return Expression.Constant(true);
            return Operations.Skip(1)
                 .Aggregate(
                     first.Combine(left),
                     (x, y) => Expression.OrElse(x, y.Combine(left))
                 );
        }
    }
}
