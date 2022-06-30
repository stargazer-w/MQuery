using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class Not : IParameterOperation
    {
        public IParameterOperation Operator { get; }

        public Not(IParameterOperation @operator)
        {
            Operator = @operator;
        }

        public Expression Combine(ParameterExpression left)
        {
            return Expression.Not(Operator.Combine(left));
        }
    }
}
