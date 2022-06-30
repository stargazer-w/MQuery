using MQuery;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MQuery.Tests
{
    [TestFixture()]
    public class PropertySelectorTests
    {
        public class Foo
        {
            public Bar Bar { get; set; } = new();
        }

        public class Bar
        {
            public int A { get; set; } = 42;
        }

        [Test]
        public void CombineTest()
        {
            var selector = new PropertySelector(typeof(Foo), "Bar", "A");
            var param = Expression.Parameter(typeof(Foo));

            var selectorExpression = selector.ToExpression(param);
            var lambda = Expression.Lambda<Func<Foo, int>>(selectorExpression, param);
            var func = lambda.Compile();
            var a = func(new Foo());

            Assert.AreEqual(selector.PropType, typeof(int));
            Assert.AreEqual(a, 42);
        }

        [Test]
        public void ShouldThrow()
        {
            Assert.Throws<ArgumentException>(
                () => new PropertySelector(typeof(Foo), "Bar", "B")
            );
        }
    }
}