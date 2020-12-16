using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQuery
{
    public class PropertyNode : IEquatable<PropertyNode>
    {
        public PropertyInfo PropertyInfo { get; }

        public PropertyNode(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public override bool Equals(object obj) => Equals(obj as PropertyNode);
        public bool Equals(PropertyNode other)
        {
            if(other is null)
                return false;

            return PropertyInfo == other.PropertyInfo;
        }

        public static bool operator ==(PropertyNode left, PropertyNode right) => Equals(left, right);
        public static bool operator !=(PropertyNode left, PropertyNode right) => !(left == right);

        public override int GetHashCode()
        {
            return PropertyInfo.GetHashCode();
        }
    }
}
