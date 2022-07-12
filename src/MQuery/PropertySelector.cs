using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MQuery
{
    public class PropertySelector<T>
    {
        public IEnumerable<string> PropertyNames { get; }
        public Type PropertyType { get; }
        /// <summary>
        /// 若属性类型是一个集合类型，则返回集合的元素类型，否则返回null
        /// </summary>
        public Type? PropertyCollectionElementType { get; }

        public PropertySelector(params string[] propertyNames)
        {
            PropertyType = typeof(T);
            for(int i = 0; i < propertyNames.Length; i++)
            {
                var name = propertyNames[i];
                var p = PropertyType.GetProperty(name);
                if(p is null)
                {
                    var nameChain = string.Join(".", propertyNames.Take(i + 1));
                    throw new ArgumentException($"can not found property {nameChain} of object");
                }
                PropertyType = p.PropertyType;
            }

            if(IsCollection(PropertyType, out var eleType))
            {
                PropertyCollectionElementType = eleType;
            }

            PropertyNames = propertyNames;
        }

        public Expression ToExpression(ParameterExpression left)
        {
            if(left.Type != typeof(T))
                throw new ArgumentException("left expression's type must equals LeftType");

            Expression selector = left;
            foreach(var prop in PropertyNames)
            {
                selector = Expression.Property(selector, prop);
            }
            return selector;
        }

        static bool IsCollection(Type type, out Type? elementType)
        {
            if(type.GetInterface("ICollection`1")?.GetGenericArguments()[0] is Type eleType)
            {
                elementType = eleType;
                return true;
            }
            else
            {
                elementType = null;
                return false;
            }
        }
    }
}
