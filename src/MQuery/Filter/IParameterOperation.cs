using System.Linq.Expressions;

namespace MQuery.Filter
{
    public interface IParameterOperation
    {
        Expression Combine(ParameterExpression left);
    }
}
