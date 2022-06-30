using MQuery.Filter;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class EqualTests
    {
        [TestCase(10, 10, true)]
        [TestCase(10, 15, false)]
        [TestCase(10, 5, false)]
        [Test]
        public void CombineTest(int left, int right, bool reuslt)
        {
            var in5and10 = new Equal<int>(right);
            var body = in5and10.Combine(Expression.Constant(left));
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            Assert.AreEqual(func(), reuslt);
        }
    }
}