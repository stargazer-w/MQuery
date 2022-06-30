using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class LessThenTests
    {
        [TestCase(10, 10, false)]
        [TestCase(10, 15, true)]
        [TestCase(10, 5, false)]
        [Test]
        public void CombineTest(int left, int right, bool reuslt)
        {
            var op = new LessThen<int>(right);
            var body = op.Combine(Expression.Constant(left));
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.AreEqual(func(), reuslt);
        }
    }
}