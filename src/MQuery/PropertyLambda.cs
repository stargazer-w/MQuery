using System;
using System.Linq.Expressions;
using MQuery.Utils;

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

            if(TypeHelper.IsCollection(PropertyType, out var eleType))
            {
                PropertyCollectionElementType = eleType;
            }
        }

        public Expression ToExpression(ParameterExpression left)
        {
            var rebinder = new ParameterRebindVisitor(left);
            return rebinder.Visit(_lambda.Body);
        }

        public static PropertyLambda<T> Create<U>(Expression<Func<T, U>> selector)
        {
            return new PropertyLambda<T>(selector);
        }
    }
}
