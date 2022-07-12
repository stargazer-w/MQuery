using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class Not : IOperator
    {
        public IOperator Operator { get; }

        public Not(IOperator @operator)
        {
            Operator = @operator;
        }

        public Expression Combine(Expression left)
        {
            return Expression.Not(Operator.Combine(left));
        }
    }
}
