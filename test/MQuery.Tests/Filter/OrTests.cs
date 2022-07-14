using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class OrTests
    {
        [TestCase(new[] { true, true }, true)]
        [TestCase(new[] { false, false }, false)]
        [TestCase(new[] { true, false }, true)]
        [Test()]
        public void CombineTest(bool[] conditions, bool result)
        {
            var propOps = conditions
                .Select(x => new LambdaOperation<object>(_ => x))
                .ToArray();
            var op = new Or(propOps);
            var param = Expression.Parameter(typeof(object));
            var body = op.Combine(param);
            var func = Expression.Lambda<Func<bool>>(body).Compile();

            func().ShouldBe(result);
        }

    }
}