using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MQuery
{
    public record PropertyNode : IEquatable<PropertyNode>
    {
        public PropertyInfo PropertyInfo { get; }

        public List<PropertyInfo> SubProertyInfos { get; } = new List<PropertyInfo>();

        public Type PropertyType => SubProertyInfos.Any() ? SubProertyInfos.Last().PropertyType : PropertyInfo.PropertyType;

        public PropertyNode(PropertyInfo propertyInfo, params PropertyInfo[] subProertyInfos)
        {
            PropertyInfo = propertyInfo;
            SubProertyInfos = subProertyInfos.ToList();
        }
    }
}
