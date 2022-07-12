using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class NotTests
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

        [TestCase(true, false)]
        [TestCase(false, true)]
        [Test]
        public void CombineTest(bool condition, bool reuslt)
        {
            var op = new Not(new ConstantOperator(condition));
            var propOp = PropertyOperation<object>.Self(op);
            var param = Expression.Parameter(typeof(object));
            var body = op.Combine(param);
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.AreEqual(func(), reuslt);
        }
    }
}