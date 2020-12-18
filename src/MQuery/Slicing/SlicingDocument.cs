using System;
using System.Collections.Generic;
using System.Text;

namespace MQuery.Slicing
{
    public class SlicingDocument
    {
        public Type ElementType { get; }

        public int? Skip { get; set; }

        public int? Limit { get; set; }

        public SlicingDocument(Type elementType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }
    }
}
