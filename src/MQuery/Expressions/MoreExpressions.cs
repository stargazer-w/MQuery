using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery.Expressions
{
    public static class MoreExpressions
    {
        public static Expression In(Expression left, Expression right)
        {
            if(left is null)
                throw new ArgumentNullException(nameof(left));
            if(right is null)
                throw new ArgumentNullException(nameof(right));

            var leftEnmType = typeof(IEnumerable<>).MakeGenericType(left.Type);
            if(!right.Type.GetInterfaces().Contains(leftEnmType))
                throw new ArgumentException("Right expression type must implement IEnumerable of left's type");

            var containsInfo = typeof(Enumerable).GetMethods()
                                                  .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                                  .MakeGenericMethod(left.Type);

            return Expression.Call(containsInfo, right, left);
        }
    }
}
