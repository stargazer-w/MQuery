namespace MQuery.QueryString
{
    internal class SortKeyData
    {
        public SortKeyData(string propSelector)
        {
            Selector = propSelector;
        }

        public string Selector { get; }
    }
}
