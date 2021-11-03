using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace MQuery.QueryString.Tests
{
    [TestFixture()]
    public class QueryParserTests
    {
        class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public Bar Bar { get; set; }
        }

        class Bar
        {
            public string Address { get; set; }

            public DateTimeOffset CreatedAt { get; set; }
        }

        [Test()]
        public void ParseSimple()
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
        public void ParseWithNoise()
        {
            var query = new QueryParser<Foo>().Parse("?a=a&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
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
        public void ParseWithNoise2()
        {
            var query = new QueryParser<Foo>().Parse("?abcde&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2");
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
        public void ParseWithErrorOp()
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
        public void ParseWithTypeError()
        {
            Should.Throw<Exception>(() => new QueryParser<Foo>().Parse("name[$ne]=Alice&age=a&$sort[age]=-1&$skip=1&$limit=2"));
        }

        [Test()]
        public void ParseWithDot()
        {
            var query = new QueryParser<Foo>().Parse("bar.address=cde&bar.createdAt[$gt]=2000-1-1");
            var source = new List<Foo>
            {
                new Foo { Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1888,5,3)} },
                new Foo { Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1964,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1204,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2001,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2021,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1983,5,3)} },
            };
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.Where(it => it.Bar.Address == "cde" && it.Bar.CreatedAt > new DateTime(2000, 1, 1))
            );
        }

        [Test()]
        public void ParseWithDateTimeOffset()
        {
            var query = new QueryParser<Foo>().Parse("bar.address=cde&bar.createdAt[$gt]=2000-1-1+08:00");
            var source = new List<Foo>
            {
                new Foo { Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1888,5,3)} },
                new Foo { Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1964,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1204,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2001,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2021,5,3)} },
                new Foo { Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1983,5,3)} },
            };
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.Where(it =>
                    it.Bar.Address == "cde"
                    && it.Bar.CreatedAt > new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(8))
                )
            );
        }
    }
}