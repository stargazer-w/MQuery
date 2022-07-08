using System.Collections.Generic;

namespace MQuery.QueryString
{
    internal class FilterKeyData
    {
        public FilterKeyData(string selector, IEnumerable<string> ops)
        {
            Selector = selector;
            Ops = ops;
        }

        public string Selector { get; }

        public IEnumerable<string> Ops { get; }
    }
}
