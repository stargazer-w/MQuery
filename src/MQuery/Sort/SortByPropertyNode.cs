using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Sort
{
    public class SortByPropertyNode
    {
        public PropertyNode Property { get; }

        public SortType Type { get; }

        public SortByPropertyNode(PropertyNode property, SortType type)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Type = type;
        }
    }
}
