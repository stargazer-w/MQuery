using System;
using System.Collections.Generic;

namespace MQuery.QueryString
{
    public class ParseException : Exception
    {
        public string Key { get; }

        public IEnumerable<string> Values { get; set; }

        public ParseException(string message, string key) : base(message)
        {
            Key = key;
        }

        public ParseException(string message, string key, Exception innerException) : base(message, innerException)
        {
            Key = key;
        }
    }
}
