using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQuery.Filter
{
    public class PropertyComparesNode
    {
        private readonly List<CompareNode> _compares;

        public PropertyNode Property{ get; }

        public IEnumerable<CompareNode> Compares => _compares.AsReadOnly();

        public PropertyComparesNode(PropertyNode property, CompareNode compare, params CompareNode[] otherCompares)
        {
            if(otherCompares is null)
                throw new ArgumentNullException(nameof(otherCompares));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            _compares = otherCompares.Prepend(compare).ToList();
        }

        public void AddCompare(CompareNode compareNode)
        {
            _compares.Add(compareNode);
        }
    }
}
