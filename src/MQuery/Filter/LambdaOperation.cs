using System;
using System.Linq.Expressions;
using MQuery.Utils;

namespace MQuery.Filter
{
    public class LambdaOperation<T> : IParameterOperation
    {
        private readonly Expression<Func<T, bool>> _expression;

        public LambdaOperation(Expression<Func<T, bool>> expression)
        {
            _expression = expression;
        }

        public Expression Combine(ParameterExpression left)
        {
            var visitor = new ParameterRebindVisitor(left);
            return visitor.Visit(_expression.Body);
        }
    }
}
