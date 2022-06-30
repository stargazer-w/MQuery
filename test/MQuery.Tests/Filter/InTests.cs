using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class InTests
    {
        [TestCase(10, new[] { 5, 10 }, true)]
        [TestCase(15, new[] { 5, 10 }, false)]
        [Test]
        public void CombineTest(int left, int[] right, bool reuslt)
        {
            var op = new In<int>(right);
            var body = op.Combine(Expression.Constant(left));
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.AreEqual(func(), reuslt);
        }
    }
}