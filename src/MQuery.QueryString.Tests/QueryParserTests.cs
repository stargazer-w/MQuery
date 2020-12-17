using NUnit.Framework;
using MQuery.QueryString;
using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using NuGet.Frameworks;
using System.Linq;

namespace MQuery.QueryString.Tests
{
    [TestFixture()]
    public class QueryParserTests
    {
        [Test()]
        public void StructureQueryStringTest()
        {
            var dict = QueryParser.StructureQueryString("a=b&c=d&e=&a=f");
            dict["a"].ShouldBe(new[] { "b", "f" });
            dict["c"].ShouldBe(new[] { "d" });
            dict["e"].ShouldBe(new[] { "" });
        }

        class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void ParseFilterTest()
        {
            var (prop, cmp) = QueryParser.ParseFilter(typeof(Foo).GetProperties(), "name[$eq]", new[] { "Alice" });
            prop.PropertyInfo.Name.ShouldBe("Name");
            cmp.Operator.ShouldBe(Filter.CompareOperator.Eq);
            cmp.Value.ShouldBe("Alice");
        }

        [Test()]
        public void ParseFilterTest2()
        {
            var (prop, cmp) = QueryParser.ParseFilter(typeof(Foo).GetProperties(), "age", new[] { "2" });
            prop.PropertyInfo.Name.ShouldBe("Age");
            cmp.Operator.ShouldBe(Filter.CompareOperator.Eq);
            cmp.Value.ShouldBe(2);
        }

        [Test()]
        public void ParseTest()
        {
            var query = QueryParser.Parse<Foo>("name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
            var source = new List<Foo>
            {
                new Foo { Name = "Alice", Age = 18 },
                new Foo { Name = "Bob", Age = 30 },
                new Foo { Name = "Carl", Age = 50 },
                new Foo { Name = "David", Age = 20 },
                new Foo { Name = "Eva", Age = 33 },
                new Foo { Name = "Frank", Age = 15 },
            };

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.Where(it => it.Name != "Alice" && it.Age >= 18 && it.Age < 40)
                      .OrderByDescending(it => it.Age)
                      .Skip(1)
                      .Take(2)
            );
        }
    }
}