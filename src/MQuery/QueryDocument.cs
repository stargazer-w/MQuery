using System;
using System.Collections.Generic;
using System.Text;
using MQuery.Filter;
using MQuery.Slicing;
using MQuery.Sort;

namespace MQuery
{
    public class QueryDocument
    {
        public Type ElementType { get; }

        public FilterDocument Filter { get; }

        public SortDocument Sort { get; }

        public SlicingDocument Slicing { get; }

        public QueryDocument(Type elementType)
        {
            ElementType = elementType;
            Filter = new FilterDocument(elementType);
            Sort = new SortDocument(elementType);
            Slicing = new SlicingDocument(elementType);
        }
    }
}
