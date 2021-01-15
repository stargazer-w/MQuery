using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Shouldly;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class PropertyExpressionExtenssionsTests
    {
        public class Foo
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public Bar Bar { get; set; }
        }

        public class Bar
        {
            public string Address { get; set; }

            public DateTime CreatedAt { get; set; }
        }

        [Test()]
        public void ToExpressionTest()
        {
            var bar = typeof(Foo).GetProperty("Bar");
            var barCreatedAt = bar.PropertyType.GetProperty("CreatedAt");
            var property = new PropertyNode(bar, barCreatedAt);
            var param = Expression.Parameter(typeof(Foo));
            var expr = property.ToExpression(param);
            var lambda = Expression.Lambda<Func<Foo, DateTime>>(expr, param);
            var time = lambda.Compile()(new Foo { Bar = new Bar { CreatedAt = new DateTime(1970, 1, 1) } });
            time.ShouldBe(new DateTime(1970, 1, 1));
        }
    }
}