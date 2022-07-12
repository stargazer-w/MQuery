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
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public Bar Bar { get; set; }

            public List<int?> Tags { get; set; } = new();
        }

        public class Bar
        {
            public string Address { get; set; }

            public DateTimeOffset CreatedAt { get; set; }
        }

        public readonly List<Foo> source = new()
        {
            new Foo { Name = "Alice", Age = 18, Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1888,5,3)}, Tags = { 1, 2 } },
            new Foo { Name = "Bob", Age = 30, Bar = new Bar{ Address = "abc", CreatedAt = new DateTime(1964,5,3)}, Tags = { 2, 3 } },
            new Foo { Name = "Carl", Age = 50, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1204,5,3)}, Tags = { 3, 4 } },
            new Foo { Name = "David", Age = 20, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2001,5,3)}, Tags = { 1, 3 } },
            new Foo { Name = "Eva", Age = 33, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(2021,5,3)}, Tags = { 2, 4 } },
            new Foo { Name = "Frank", Age = 15, Bar = new Bar{ Address = "cde", CreatedAt = new DateTime(1983,5,3)}, Tags = null },
        };

        [Test()]
        public void ParseEq()
        {
            const string queryString = "name[$eq]=Alice";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name == "Alice")
            );
        }

        [Test()]
        public void ParseNe()
        {
            const string queryString = "name[$ne]=Alice";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice")
            );
        }

        [Test()]
        public void ParseGt()
        {
            const string queryString = "Age[$gt]=20";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Age > 20)
            );
        }

        [Test()]
        public void ParseGte()
        {
            const string queryString = "Age[$gte]=20";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Age >= 20)
            );
        }

        [Test()]
        public void ParseLt()
        {
            const string queryString = "Age[$lt]=20";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Age < 20)
            );
        }

        [Test()]
        public void ParseLte()
        {
            const string queryString = "Age[$lte]=20";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Age <= 20)
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
        public void ParseAny()
        {
            const string queryString = "tags[$any][$gt]=1";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.AsQueryable()
                .Where(it => it.Tags != null && it.Tags.Any(x => x > 1))
            );
        }


        [Test()]
        public void ParseAnyWithAnd()
        {
            const string queryString = "tags[$any][$gt]=1&tags[$any][$lt]=3";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            // BUG
            result.ShouldNotBe(source.AsQueryable()
                 .Where(it => it.Tags != null && it.Tags.Any(x => x > 1 && x < 3))
             );
        }

        [Test()]
        public void ParseNot()
        {
            const string queryString = "name[$not][$eq]=Alice";

            var query = new QueryParser<Foo>().Parse(queryString);
            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice")
            );
        }

        [Test()]
        public void ParseDotPropertySelector()
        {
            const string queryString = "bar.address[$eq]=cde";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Bar.Address == "cde")
            );
        }

        [Test()]
        public void ParseWithNoise()
        {
            const string queryString = "?a=a&name[$eq]=Alice";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name == "Alice")
            );
        }

        [Test()]
        public void ParseWithErrorOp()
        {
            const string queryString = "name[$]=Alice&name[$eq]=Alice";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name == "Alice")
            );
        }

        [Test()]
        public void ParseWithTypeError()
        {
            Should.Throw<Exception>(() => new QueryParser<Foo>().Parse("age[$eq]=a"));
        }

        [Test()]
        public void ParseWithIcludedProps()
        {
            const string queryString = "name[$ne]=Alice&age[$gte]=18";
            var query = new QueryParser<Foo>(new() { IncludeProps = new() { "name" } }).Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice")
            );
        }

        [Test]
        public void ParseDefautEq()
        {
            const string queryString = "name=Alice";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name == "Alice")
            );
        }

        [Test]
        public void ParseDefautEqWithNot()
        {
            const string queryString = "name[$not]=Alice";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Name != "Alice")
            );
        }

        [Test]
        public void ParseDefautEqWithAny()
        {
            const string queryString = "tags[$any]=1";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags != null && it.Tags.Any(x => x == 1))
            );
        }

        [Test]
        public void ParseNin()
        {
            const string queryString = "name[$nin]=Alice";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => !new[] { "Alice" }.Contains(it.Name))
            );
        }

        [Test]
        public void ParseDefaultAny()
        {
            const string queryString = "tags=1";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags != null && it.Tags.Any(x => x == 1))
            );
        }

        [Test]
        public void ParseArrayNullQuery()
        {
            const string queryString = "tags[$eq]=";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags == null)
            );
        }

        [Test]
        public void ParseArrayAnyNullQuery()
        {
            const string queryString = "tags[$any][$eq]=";
            var query = new QueryParser<Foo>().Parse(queryString);

            var result = query.ApplyTo(source.AsQueryable());

            result.ShouldBe(
                source.AsQueryable().Where(it => it.Tags != null && it.Tags.Any(x => x == null))
            );
        }
    }
}