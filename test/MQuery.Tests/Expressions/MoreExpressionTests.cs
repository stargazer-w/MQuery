using NUnit.Framework;
using MQuery.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Shouldly;

namespace MQuery.Expressions.Tests
{
    [TestFixture()]
    public class MoreExpressionTests
    {
        [Test()]
        public void InTest()
        {
            var inExpr = MoreExpressions.In(Expression.Constant(1), Expression.Constant(new[] { 1 }));

            Expression.Lambda<Func<bool>>(inExpr).Compile()().ShouldBe(true);
        }
    }
}