using System.Collections.Generic;

namespace MQuery.QueryString
{
    public class ParserOptions
    {
        public int? DefaultLimit { get; set; }

        public int? MaxLimit { get; set; }

        public List<string>? IncludeProps { get; set; }

        public List<IValueParser> ValueParsers { get; set; } = new();
    }
}
