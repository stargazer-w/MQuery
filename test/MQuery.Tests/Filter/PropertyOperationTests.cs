using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class PropertyOperationTests
    {
        public class ConstantOperator : IOperator
        {
            private readonly object _value;

            public ConstantOperator(object value)
            {
                _value = value;
            }

            public Expression Combine(Expression left)
            {
                return Expression.Constant(_value);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        [Test()]
        public void CombineTest(bool should)
        {
            var operation = PropertyOperation<object>.Self(new ConstantOperator(should));

            var param = Expression.Parameter(typeof(object));
            var body = operation.Combine(param);
            var expr = Expression.Lambda<Func<object?, bool>>(body, param);
            var result = expr.Compile()(null);

            result.ShouldBe(should);
        }
    }
}