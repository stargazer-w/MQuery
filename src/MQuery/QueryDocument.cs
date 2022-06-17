using MQuery.Filter;
using MQuery.Slicing;
using MQuery.Sort;

namespace MQuery
{
    public class QueryDocument<T>
    {
        public FilterDocument<T> Filter { get; } = new();

        public SortDocument<T> Sort { get; } = new();

        public SlicingDocument Slicing { get; } = new();
    }
}
