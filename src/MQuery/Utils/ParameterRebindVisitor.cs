using System.Linq.Expressions;

namespace MQuery.Utils
{
    internal class ParameterRebindVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterRebindVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
