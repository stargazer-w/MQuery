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

            public List<string> Tags { get; set; } = new();
        }

        class Bar
        {
            public string Address { get; set; }

            public DateTimeOffset CreatedAt { get; set; }
        }

        readonly List<Foo> source = new List<Foo>
        {
            new Foo { Name = "Alice", Age = 18, Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1888,5,3)}, Tags = { "A", "B" } },
            new Foo { Name = "Bob", Age = 30, Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1964,5,3)}, Tags = { "C", "B" } },
            new Foo { Name = "Carl", Age = 50, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1204,5,3)}, Tags = { "D", "C" } },
            new Foo { Name = "David", Age = 20, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2001,5,3)}, Tags = { "A", "C" } },
            new Foo { Name = "Eva", Age = 33, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2021,5,3)}, Tags = { "B", "D" } },
            new Foo { Name = "Frank", Age = 15, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1983,5,3)}, Tags = null },
        };

        [Test()]
        public void ParseSimple()
        {
            const string queryString = "name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice" && it.Age >= 18 && it.Age < 40)
                      .OrderByDescending(it => it.Age)
                      .Skip(1)
                      .Take(2)
            );
        }

        [Test()]
        public void ParseIn()
        {
            const string queryString = "name[$in][]=Alice&name[$in][]=Bob";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.AsQueryable().Where(it => new[] { "Alice", "Bob" }.Contains(it.Name)));
        }

        [Test()]
        public void ParseWithNoise()
        {
            const string queryString = "?a=a&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice" && it.Age >= 18 && it.Age < 40)
                      .OrderByDescending(it => it.Age)
                      .Skip(1)
                      .Take(2)
            );
        }

        [Test()]
        public void ParseWithNoise2()
        {
            const string queryString = "?abcde&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice" && it.Age >= 18 && it.Age < 40)
                      .OrderByDescending(it => it.Age)
                      .Skip(1)
                      .Take(2)
            );
        }

        [Test()]
        public void ParseWithErrorOp()
        {
            const string queryString = "name[$]=Alice&name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice" && it.Age >= 18 && it.Age < 40)
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
            const string queryString = "bar.address=cde&bar.createdAt[$gt]=2000-1-1";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Bar.Address == "cde" && it.Bar.CreatedAt > new DateTime(2000, 1, 1))
            );
        }

        [Test()]
        public void ParseWithDateTimeOffset()
        {
            const string queryString = "bar.address=cde&bar.createdAt[$gt]=2000-1-1+08:00";
            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it =>
                    it.Bar.Address == "cde"
                    && it.Bar.CreatedAt > new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(8))
                )
            );
        }

        [Test()]
        public void ParseWithIcludedProps()
        {
            const string queryString = "name[$ne]=Alice&age[$gte]=18&age[$lt]=40&$sort[age]=-1&$skip=1&$limit=2";
            var query = new QueryParser<Foo>(new() { IncludeProps = new() { "Name" } }).Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice")
                      .Skip(1)
                      .Take(2)
            );
        }

        [Test]
        public void ParseWithArrayQuery()
        {
            const string queryString = "tags=A";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags != null && it.Tags.Any(x => x == "A"))
            );
        }

        [Test]
        public void ParseWithArrayNullQuery()
        {
            const string queryString = "tags=";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags == null)
            );
        }

        [Test]
        public void ParseWithArrayAnyNullQuery()
        {
            const string queryString = "tags[$any]=";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags != null && it.Tags.Any(x => x == null))
            );
        }
    }
}