using MQuery.Filter;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class FilterExpressionExtensionsTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void Gt()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();
            filter.AddPropertyCompare(x => x.Age, CompareOperator.Gt, 18);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age > 18));
        }

        [Test()]
        public void In()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();
            filter.AddPropertyCompare(x => x.Age, CompareOperator.In, new[] { 18 });

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void Eq()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();
            filter.AddPropertyCompare(x => x.Age, CompareOperator.Eq, 18);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age == 18));
        }

        [Test()]
        public void Nin()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();
            filter.AddPropertyCompare(x => x.Age, CompareOperator.Nin, new[] { 18 });

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => !new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void And()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();
            filter.AddPropertyCompare(x => x.Age, CompareOperator.Gte, 18);
            filter.AddPropertyCompare(x => x.Age, CompareOperator.Lte, 30);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age is >= 18 and <= 30));
        }

        [Test()]
        public void Empty()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 30 }, new Foo { Name = "Carl", Age = 50 } };
            var type = typeof(Foo);
            var filter = new FilterDocument<Foo>();

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source);
        }

        public class Bar
        {
            public Foo Foo { get; set; }
        }

        [Test]
        public void NestedEq()
        {
            var source = new List<Bar>
            {
                new() { Foo = new Foo { Name = "Alice", Age = 18 } },
                new() { Foo = new Foo { Name = "Bob", Age = 30 } },
                new() { Foo = new Foo { Name = "Carl", Age = 50 } }
            };
            var barType = typeof(Bar);
            var fooType = typeof(Foo);
            var filter = new FilterDocument<Bar>();
            filter.AddPropertyCompare(x => x.Foo.Age, CompareOperator.Eq, 18);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Foo.Age == 18));
        }

        public class NullableValueProp
        {
            public int? Foo { get; set; }
        }

        [Test]
        public void NullableValueCompare()
        {
            var source = new List<NullableValueProp>
            {
                new() { Foo = 0 },
                new() { Foo = 1 },
                new() { Foo = 2 },
                new() { Foo = 3 },
            };

            var type = typeof(NullableValueProp);
            var filter = new FilterDocument<NullableValueProp>();
            filter.AddPropertyCompare(x => x.Foo, CompareOperator.Eq, (int?)0);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.Foo == 0));
        }

        public class CollectionProp
        {
            public List<int> List { get; set; } = null;
        }

        [Test]
        public void CollectionEqualNull_Should_CompareSelf()
        {
            var obj = new CollectionProp();
            var source = new List<CollectionProp> { obj, new() { List = new() } };

            var filter = new FilterDocument<CollectionProp>();
            filter.AddPropertyCompare(x => x.List, CompareOperator.Eq, (List<int>)null);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.List == null));
        }

        [Test]
        public void CollectionCompare_Should_CompareAnyElement()
        {
            var source = new List<CollectionProp>
            {
                new() { List = new(){ 15,20 } },
                new() { List = new(){ 20,25 } }
            };

            var filter = new FilterDocument<CollectionProp>();
            filter.AddPropertyCompare(x => x.List, CompareOperator.Gt, 25);

            var expr = filter.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(x => x.List.Any(x => x > 25)));
        }
    }
}