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
        class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void ParsePass()
        {
            var query = new QueryParser<Foo>().Parse("name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
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

        [Test()]
        public void ParseIn()
        {
            var query = new QueryParser<Foo>().Parse("name[$in][]=Alice&name[$in][]=Bob");
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

            result.ShouldBe(source.Where(it => new[] { "Alice", "Bob" }.Contains(it.Name)));
        }

        [Test()]
        public void ParsePass2()
        {
            var query = new QueryParser<Foo>().Parse("a=a&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
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

        [Test()]
        public void ParsePass3()
        {
            var query = new QueryParser<Foo>().Parse("name[$]=Alice&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
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


        [Test()]
        public void ParseFail()
        {
            Should.Throw<Exception>(() => new QueryParser<Foo>().Parse("name[$ne]=Alice&age=a&$sort[age]=-1&$skip=1&$limit=2"));
        }
    }
}