using System.Linq.Expressions;

namespace MQuery.Filter
{
    public class PropertyOperation : IParameterOperation
    {
        public PropertySelector Selector { get; }

        public IOperator Operator { get; }

        public PropertyOperation(PropertySelector selector, IOperator @operator)
        {
            Selector = selector;
            Operator = @operator;
        }

        public Expression Combine(ParameterExpression left)
        {
            return Operator.Combine(Selector.ToExpression(left));
        }

        public static PropertyOperation Self<T>(IOperator @operator)
        {
            return new PropertyOperation(new PropertySelector(typeof(T)), @operator);
        }
    }

    public interface IParameterOperation
    {
        Expression Combine(ParameterExpression left);
    }
}
