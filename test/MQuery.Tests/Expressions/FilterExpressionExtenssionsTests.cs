﻿using NUnit.Framework;
using MQuery.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Shouldly;
using MQuery.Filter;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class FilterExpressionExtenssionsTests
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
            var filter = new FilterDocument(type);
            filter.AddPropertyCompare(type.GetProperty("Age"), CompareOperator.Gt, 18);

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age > 18));
        }

        [Test()]
        public void In()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument(type);
            filter.AddPropertyCompare(type.GetProperty("Age"), CompareOperator.In, new[] { 18 });

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void Eq()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument(type);
            filter.AddPropertyCompare(type.GetProperty("Age"), CompareOperator.Eq, 18);

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age == 18));
        }

        [Test()]
        public void Nin()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument(type);
            filter.AddPropertyCompare(type.GetProperty("Age"), CompareOperator.Nin, new[] { 18 });

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => !new[] { 18 }.Contains(it.Age)));
        }

        [Test()]
        public void And()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 20 } };
            var type = typeof(Foo);
            var filter = new FilterDocument(type);
            var ageProp = type.GetProperty("Age");
            filter.AddPropertyCompare(ageProp, CompareOperator.Gte, 18);
            filter.AddPropertyCompare(ageProp, CompareOperator.Lte, 30);

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age is >= 18 and <= 30));
        }

        [Test()]
        public void Empty()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 30 }, new Foo { Name = "Carl", Age = 50 } };
            var type = typeof(Foo);
            var filter = new FilterDocument(type);

            var expr = filter.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source);
        }
    }
}