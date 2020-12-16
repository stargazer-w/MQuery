using NUnit.Framework;
using MQuery.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQuery.Sort;
using System.Linq.Expressions;
using Shouldly;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class SortExpressionExtenssionsTests
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
            var type = typeof(Foo);
            var sort = new SortDocument(type);
            var ageProp = type.GetProperty("Age");
            sort.AddSortByProperty(new(ageProp), SortType.Desc);

            var expr = sort.ToExpression();
            var result = ((Expression<Func<IQueryable<Foo>, IQueryable<Foo>>>)expr).Compile()(source.AsQueryable());

            result.ShouldBe(source.AsQueryable().Reverse());
        }
    }
}