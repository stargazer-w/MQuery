using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class And : IParameterOperation
    {
        public IList<IParameterOperation> Operators { get; }

        public And(params IParameterOperation[] operators)
        {
            Operators = operators.ToList();
        }

        public Expression Combine(ParameterExpression left)
        {
            var first = Operators.FirstOrDefault();
            if(first == null)
                return Expression.Constant(true);
            return Operators.Skip(1)
                 .Aggregate(
                     first.Combine(left),
                     (x, y) => Expression.And(x, y.Combine(left))
                 );
        }
    }
}
