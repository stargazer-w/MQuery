using System;
using System.Linq.Expressions;

namespace MQuery
{
    public class PropertyLambda<T> : IPropertySelector<T>
    {
        private readonly LambdaExpression _lambda;

        public Type? PropertyCollectionElementType { get; }
        public Type PropertyType => _lambda.ReturnType;

        internal PropertyLambda(LambdaExpression lambda)
        {
            _lambda = lambda;
        }

        public Expression ToExpression(ParameterExpression left)
        {
            var rebinder = new ParameterRebindVisitor(left);
            return rebinder.Visit(_lambda.Body);
        }


        class ParameterRebindVisitor : ExpressionVisitor
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

    public static class PropertyLambda
    {
        public static PropertyLambda<T> Create<T, U>(Expression<Func<T, U>> selector)
        {
            return new PropertyLambda<T>(selector);
        }
    }
}
