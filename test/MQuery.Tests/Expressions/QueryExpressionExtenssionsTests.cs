﻿using MQuery.Filter;
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
    public class QueryExpressionExtenssionsTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [Test()]
        public void ToExpressionTest()
        {
            var source = new List<Foo>
            {
                new Foo { Name = "Alice", Age = 18 },
                new Foo { Name = "Bob", Age = 30 },
                new Foo { Name = "Carl", Age = 50 },
                new Foo { Name = "David", Age = 20 },
                new Foo { Name = "Eva", Age = 33 },
                new Foo { Name = "Frank", Age = 15 },
            };
            var query = new QueryDocument<Foo>();
            var ageProp = typeof(Foo).GetProperty("Age");
            query.Filter.AddPropertyCompare(ageProp, CompareOperator.Gt, 18);
            query.Filter.AddPropertyCompare(ageProp, CompareOperator.Lte, 40);
            query.Sort.AddSortByProperty(new(ageProp), SortPattern.Desc);
            query.Slicing.Skip = 1;
            query.Slicing.Limit = 2;

            var expr = query.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.Where(it => it.Age > 18 && it.Age < 40).OrderByDescending(it => it.Age).Skip(1).Take(2));
        }
    }
}
