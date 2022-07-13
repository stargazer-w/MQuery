using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class AnyTests
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

        [TestCase(true, true)]
        [TestCase(false, false)]
        [Test()]
        public void CombineTest(bool condition, bool result)
        {
            var propOp = PropertyOperation<object>.Self(new ConstantOperator(condition));
            var op = new Any(propOp);
            var body = op.Combine(Expression.Constant(new object[] { null }));
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.That(func(), Is.EqualTo(result));
        }

        [TestCase(true, false)]
        [TestCase(false, false)]
        [Test()]
        public void CombineTestWithNull(bool condition, bool result)
        {
            var propOp = PropertyOperation<object>.Self(new ConstantOperator(condition));
            var op = new Any(propOp);
            var body = op.Combine(Expression.Constant(null, typeof(object).MakeArrayType()));
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.That(func(), Is.EqualTo(result));
        }
    }
}