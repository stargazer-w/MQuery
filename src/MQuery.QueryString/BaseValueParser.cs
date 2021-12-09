using System;
using System.ComponentModel;

namespace MQuery.QueryString
{
    public class BaseValueParser : IValueParser
    {
        public bool CanParse(Type type)
        {
            if(type == typeof(string))
                return true;
            var typeConverter = TypeDescriptor.GetConverter(type);
            return typeConverter.CanConvertFrom(typeof(string));
        }

        public object? Parse(string? valueStr, Type type)
        {
            if(type == typeof(string))
                return valueStr;

            var typeConverter = TypeDescriptor.GetConverter(type);
            return typeConverter.ConvertFromString(valueStr);
        }
    }

    public class ClassNullValueParser : IValueParser
    {
        public bool CanParse(Type type)
        {
            return type.IsClass;
        }

        public object? Parse(string? valueStr, Type type)
        {
            if(valueStr == null)
                return null;
            else
                throw new NotSupportedException($"Type {type} can not be convert from query");
        }
    }
}
