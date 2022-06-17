using MQuery.Filter;
using MQuery.Slice;
using MQuery.Sort;

namespace MQuery
{
    public class QueryDocument<T>
    {
        public FilterDocument<T> Filter { get; } = new();

        public SortDocument<T> Sort { get; } = new();

        public SliceDocument Slice { get; } = new();
    }
}
