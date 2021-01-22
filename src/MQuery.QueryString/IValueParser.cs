using System;

namespace MQuery.QueryString
{
    public interface IValueParser
    {
        bool CanParse(Type type);
        object? Parse(string? valueStr, Type type);
    }
}