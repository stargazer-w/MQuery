using System.Collections.Generic;
using System.Linq;
using MQuery.Builders;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Filter.Tests
{
    [TestFixture()]
    public class FilterDocumentTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public double? Salary { get; set; }

            public Foo Other { get; set; }
        }

        public static List<Foo> source = new()
        {
            new Foo { Name = "Alice", Age = 18, Salary = null, Other = new Foo { Name = "Bob",  Age = 30 } },
            new Foo { Name = "Bob",   Age = 20, Salary = 1000, Other = new Foo { Name = "Carl", Age = 50 } },
        };

        [Test()]
        public void Empty()
        {
            var filter = new FilterBuilder<Foo>().Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source);
        }

        [Test()]
        public void Gt()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Age").Gt(18)
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age > 18));
        }

        [Test()]
        public void In()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Age").In(new[] { 18 })
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void Eq()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Age").Eq(18)
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age == 18));
        }

        [Test()]
        public void Nin()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Age").Not().In(new[] { 18 })
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(it => !new[] { 18 }.Contains(it.Age)));
        }

        [Test]
        public void NestedEq()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Other", "Age").Eq(18)
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Other.Age == 18));
        }

        [Test]
        public void NullableValueCompare()
        {
            var filter = new FilterBuilder<Foo>()
                .Prop("Salary").Eq((double?)1000)
                .Build();
            var result = filter.ApplyTo(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Salary == 1000));
        }
    }
}