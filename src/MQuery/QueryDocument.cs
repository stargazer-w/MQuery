using MQuery.Filter;
using MQuery.Slicing;
using MQuery.Sort;

namespace MQuery
{
    public class QueryDocument<T>
    {
        public FilterDocument Filter { get; } = new(typeof(T));

        public SortDocument Sort { get; } = new(typeof(T));

        public SlicingDocument Slicing { get; } = new();
    }
}
