using System;
using System.Collections.Generic;
using System.Linq;

namespace MQuery.QueryString
{
    public class ParseException : Exception
    {
        public string? Key { get; set; }

        public IEnumerable<string> Values { get; set; } = Enumerable.Empty<string>();

        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
