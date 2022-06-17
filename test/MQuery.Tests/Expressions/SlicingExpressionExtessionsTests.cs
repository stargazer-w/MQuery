using MQuery.Slicing;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class SlicingExpressionExtessionsTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void ToExpressionTest()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 30 }, new Foo { Name = "Carl", Age = 50 } };
            var type = typeof(Foo);
            var slicing = new SlicingDocument()
            {
                Skip = 1,
                Limit = 1,
            };

            var expr = slicing.ToExpression<Foo>();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Skip(1).Take(1));
        }
    }
}