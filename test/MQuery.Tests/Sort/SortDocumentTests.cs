using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Sort.Tests
{
    [TestFixture()]
    public class SortDocumentTests
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
            var sort = new SortDocument<Foo>();
            sort.SortBys.Add(new(PropertyLambda<Foo>.Create(x => x.Age), SortPattern.Desc));

            var expr = sort.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.AsQueryable().OrderByDescending(x => x.Age));
        }
    }
}