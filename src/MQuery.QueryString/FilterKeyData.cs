namespace MQuery.QueryString
{
    internal class FilterKeyData
    {
        public FilterKeyData(string propSelector, string op)
        {
            PropSelector = propSelector;
            Op = op;
        }

        public string PropSelector { get; }

        public string Op { get; }
    }
}
