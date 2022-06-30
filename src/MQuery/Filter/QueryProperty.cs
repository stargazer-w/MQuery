using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class QueryProperty : IParameterOperation
    {
        public PropertySelector Selector { get; }

        public IOperator Operator { get; }

        public QueryProperty(PropertySelector selector, IOperator @operator)
        {
            Selector = selector;
            Operator = @operator;
        }

        public Expression Combine(ParameterExpression left)
        {
            return Operator.Combine(Selector.ToExpression(left));
        }

        public static QueryProperty Self<T>(IOperator @operator)
        {
            return new QueryProperty(new PropertySelector(typeof(T)), @operator);
        }
    }

    public interface IParameterOperation
    {
        Expression Combine(ParameterExpression left);
    }
}
