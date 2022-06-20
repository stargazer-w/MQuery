namespace MQuery.QueryString
{
    internal class SliceKeyData
    {
        public SliceKeyData(SliceSetTo sliceSetTo)
        {
            SliceSetTo = sliceSetTo;
        }

        public SliceSetTo SliceSetTo { get; }
    }

    enum SliceSetTo
    {
        Skip,
        Limit,
    }
}
