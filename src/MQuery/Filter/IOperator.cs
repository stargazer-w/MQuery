using System.Linq.Expressions;

namespace MQuery.Filter
{
    public interface IOperator
    {
        Expression Combine(Expression left);
    }
}
