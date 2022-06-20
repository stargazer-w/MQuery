namespace MQuery.QueryString
{
    internal class SortKeyData
    {
        public SortKeyData(string propSelector)
        {
            PropSelector = propSelector;
        }

        public string PropSelector { get; }
    }
}
