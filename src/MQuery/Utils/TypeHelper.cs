using System;

namespace MQuery.Utils
{
    internal static class TypeHelper
    {
        public static bool IsCollection(Type type, out Type? elementType)
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
