using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Slice.Tests
{
    [TestFixture()]
    public class SliceDocumentTests
    {
        public class Foo
        {
            public string? Name { get; set; }

            public int Age { get; set; }

            public double? Salary { get; set; }

            public Foo? Other { get; set; }
        }

        public static List<Foo> source = new()
        {
            new Foo { Name = "Alice", Age = 18, Salary = null, Other = new Foo { Name = "Bob",  Age = 30 } },
            new Foo { Name = "Bob",   Age = 20, Salary = 1000, Other = new Foo { Name = "Carl", Age = 50 } },
        };

        [Test()]
        public void ToExpressionTest()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 30 }, new Foo { Name = "Carl", Age = 50 } };
            var type = typeof(Foo);
            var slice = new SliceDocument()
            {
                Skip = 1,
                Limit = 1,
            };

            var expr = slice.ToExpression<Foo>();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Skip(1).Take(1));
        }
    }
}