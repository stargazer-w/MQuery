using MQuery.Sort;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class SortExpressionExtensionsTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void ToExpressionTest()
        {
            var source = new List<Foo> { new Foo { Name = "Alice", Age = 18 }, new Foo { Name = "Bob", Age = 30 }, new Foo { Name = "Carl", Age = 50 } };
            var sort = new SortDocument<Foo>();
            sort.AddSortByProperty(x => x.Age, SortPattern.Desc);

            var expr = sort.ToExpression();
            var result = expr.Compile()(source.AsQueryable());

            result.ShouldBe(source.AsQueryable().OrderByDescending(x => x.Age));
        }
    }
}