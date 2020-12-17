using System;
using System.Collections.Generic;

namespace MQuery.QueryString
{
    public class ParseExpception : Exception
    {
        public string Key { get; }

        public IEnumerable<string> Values { get; set; }

        public ParseExpception(string message, string key) : base(message)
        {
            Key = key;
        }

        public ParseExpception(string message, string key, Exception innerException) : base(message, innerException)
        {
            Key = key;
        }
    }
}
