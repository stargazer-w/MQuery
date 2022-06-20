namespace MQuery.QueryString
{
    public class MatchResult<T>
    {
        public MatchResult(bool isMathed, T? value)
        {
            IsMathed = isMathed;
            Value = value;
        }

        public bool IsMathed { get; }

        public T? Value { get; }
    }

    public static class MatchResult
    {
        public static MatchResult<T> Matched<T>(T value)
        {
            return new MatchResult<T>(true, value);
        }

        public static MatchResult<T> Unmatched<T>()
        {
            return new MatchResult<T>(false, default);
        }
    }
}
