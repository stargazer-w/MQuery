using System;
using System.Linq.Expressions;

namespace MQuery.Sort
{
    public class SortByPropertyNode
    {
        public LambdaExpression PropertySelector { get; }

        public SortPattern Type { get; }

        public SortByPropertyNode(LambdaExpression selector, SortPattern pattern)
        {
            PropertySelector = selector ?? throw new ArgumentNullException(nameof(selector));
            Type = pattern;
        }
    }
}
