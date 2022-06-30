using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class AndTests
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

        [TestCase(new[] { true, true }, true)]
        [TestCase(new[] { false, false }, false)]
        [TestCase(new[] { true, false }, false)]
        [Test()]
        public void CombineTest(bool[] conditions, bool result)
        {
            var propOps = conditions
                .Select(x => new ConstantOperator(x))
                .Select(QueryProperty.Self<object>)
                .ToArray();
            var op = new And(propOps);
            var param = Expression.Parameter(typeof(object));
            var body = op.Combine(param);
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.That(func(), Is.EqualTo(result));
        }
    }
}