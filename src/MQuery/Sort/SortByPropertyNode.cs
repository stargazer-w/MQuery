using System;
using System.Linq.Expressions;

namespace MQuery.Sort
{
    public class SortByPropertyNode
    {
        public LambdaExpression PropertySelector { get; }

        public SortPattern Pattern { get; }

        public SortByPropertyNode(LambdaExpression selector, SortPattern pattern)
        {
            PropertySelector = selector ?? throw new ArgumentNullException(nameof(selector));
            Pattern = pattern;
        }
    }
}
