using System.Collections.Generic;
using System.Linq;
using MQuery.Extensions;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class FilterDocumentTests
    {
        public class Foo
        {
            public string? Name { get; set; }

            public int Age { get; set; }

            public double? Salary { get; set; }

            public string[]? Tags { get; set; }

            public Foo? Other { get; set; }
        }

        public static List<Foo> source = new()
        {
            new Foo { Name = "Alice", Age = 18, Salary = null, Tags = new[]{"A"}, Other = new Foo { Name = "Bob",  Age = 30 } },
            new Foo { Name = "Bob",   Age = 20, Salary = 1000, Tags= null, Other = new Foo { Name = "Carl", Age = 50 } },
        };

        [Test()]
        public void Empty()
        {
            var filter = new FilterDocument<Foo>();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source);
        }

        [Test()]
        public void Gt()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Age, new GreaterThen<int>(18));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age > 18));
        }

        [Test()]
        public void In()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Age, new In<int>(new[] { 18 }));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void Eq()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Age, new Equal<int>(18));

            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age == 18));
        }

        [Test()]
        public void Nin()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Age, new Not(new In<int>(new[] { 18 })));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => !new[] { 18 }.Contains(it.Age)));
        }

        [Test]
        public void NestedEq()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Other!.Age, new Equal<int>(18));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Other!.Age == 18));
        }

        [Test]
        public void NullableValueCompare()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Salary, new Equal<double?>(1000));

            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Salary == 1000));
        }

        [Test]
        public void Any()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Tags, new Any(PropertyOperation<string>.Self(new Equal<string>("A"))));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Tags != null && x.Tags.Any(y => y == "A")));
        }

        [Test]
        public void All()
        {
            var filter = new FilterDocument<Foo>()
                .And(x => x.Tags, new All(PropertyOperation<string>.Self(new Equal<string>("A"))));
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Tags == null || x.Tags.All(y => y == "A")));
        }
    }
}