using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MQuery.Filter;
using MQuery.Slice;
using MQuery.Sort;

namespace MQuery.QueryString
{

    public class QueryParser<T>
    {
        private readonly List<string>? _includeProps;
        private readonly List<IValueParser> _valueParsers = new();
        private readonly int? _defaultLimit;
        private readonly int? _maxLimit;

        public QueryParser() : this(new())
        {
        }

        public QueryParser(ParserOptions options)
        {
            _includeProps = options.IncludeProps;
            _valueParsers.Add(new BaseValueParser());
            _valueParsers.Add(new ClassNullValueParser());
            _valueParsers.InsertRange(0, options.ValueParsers);
            _defaultLimit = options.DefaultLimit;
            _maxLimit = options.MaxLimit;
        }

        public Query<T> Parse(string queryString)
        {
            if(queryString is null)
                throw new ArgumentNullException(nameof(queryString));

            if(queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            var query = new Query<T>();
            var parameters = Utils.StructureQueryString(queryString);
            var props = typeof(T).GetProperties();
            foreach(var pair in parameters.Where(it => !string.IsNullOrEmpty(it.Key)))
            {
                try
                {
                    if(MatchFilter(pair.Key) is { IsMathed: true, Value: { PropSelector: var filterPS, Op: var op, } })
                    {
                        SetFilter(query.Document.Filter, filterPS, op, pair.Value);
                    }
                    else if(MatchSort(pair.Key) is { IsMathed: true, Value.PropSelector: var sortPS })
                    {
                        SetSort(query.Document.Sort, sortPS, pair.Value.First());
                    }
                    else if(MatchSlice(pair.Key) is { IsMathed: true, Value.SliceSetTo: var setTo })
                    {
                        SetSlice(query.Document.Slice, setTo, pair.Value.First());
                    }
                }
                catch(ArgumentException e)
                {
                    throw new ParseException(e.Message, e.InnerException) { Key = pair.Key, Values = pair.Value };
                }
            }

            query.Document.Slice.Limit ??= _defaultLimit;
            if(query.Document.Slice.Limit > _maxLimit)
                query.Document.Slice.Limit = _maxLimit;

            return query;
        }

        private MatchResult<SliceKeyData> MatchSlice(string key)
        {
            return key switch
            {
                "$skip" => MatchResult.Matched(new SliceKeyData(SliceSetTo.Skip)),
                "$limit" => MatchResult.Matched(new SliceKeyData(SliceSetTo.Limit)),
                _ => MatchResult.Unmatched<SliceKeyData>(),
            };
        }

        private void SetSlice(SliceDocument slice, SliceSetTo setTo, string valueString)
        {
            if(!int.TryParse(valueString, out var value))
                throw new ArgumentException($"$limit value must be integer");

            switch(setTo)
            {
                case SliceSetTo.Skip:
                    slice.Skip = value;
                    break;
                case SliceSetTo.Limit:
                    slice.Limit = value;
                    break;
            }
        }

        private MatchResult<SortKeyData> MatchSort(string key)
        {
            var match = Regex.Match(key, @"^\$sort\[([\w\.]+)\]");
            if(!match.Success)
                return MatchResult.Unmatched<SortKeyData>();

            var propSelector = match.Groups[1].Value;
            return MatchResult.Matched(new SortKeyData(propSelector));
        }

        public void SetSort(SortDocument<T> sort, string selectorString, string patternString)
        {
            if(_includeProps != null && !_includeProps.Contains(selectorString, StringComparer.OrdinalIgnoreCase))
                return;

            var propSelector = Utils.StringToPropSelector<T>(selectorString);

            if(!int.TryParse(patternString, out int patternInt))
                throw new ArgumentException($"$sort value must be integer");

            if(patternInt == 0)
                return;

            var pattern = patternInt switch
            {
                > 0 => SortPattern.Acs,
                < 0 => SortPattern.Desc,
                _ => throw new NotImplementedException(),
            };
            var sortByPropertyNode = new SortByPropertyNode(propSelector, pattern);
            sort.AddSortByProperty(sortByPropertyNode);
        }

        private MatchResult<FilterKeyData> MatchFilter(string key)
        {
            var match = Regex.Match(key, @"^([\w\.]+)(\[\$(.+?)\])?(\[\d*\])?$");
            if(!match.Success)
                return MatchResult.Unmatched<FilterKeyData>();

            var propSelector = match.Groups[1].Value;
            var op = match.Groups[3] switch
            {
                { Success: true, Value: var v } => v,
                _ => "eq"
            };

            return MatchResult.Matched(new FilterKeyData(propSelector, op));
        }

        private void SetFilter(FilterDocument<T> filter, string selectorString, string opString, IEnumerable<string> valueStrings)
        {
            if(_includeProps != null && !_includeProps.Contains(selectorString, StringComparer.OrdinalIgnoreCase))
                return;

            LambdaExpression selector;
            try
            {
                selector = Utils.StringToPropSelector<T>(selectorString);
            }
            catch
            {
                return;
            }

            if(!Enum.TryParse<CompareOperator>(opString, true, out var op))
                return;

            var compareNode = CreateCompareNode(selector, op, valueStrings);
            filter.AddPropertyCompare(compareNode);
        }

        private IPropertyComparesNode CreateCompareNode(LambdaExpression propSelector, CompareOperator op, IEnumerable<string> valueStrings)
        {
            var parseType = propSelector.ReturnType switch
            {
                var x when x.GetInterface("ICollection`1") is { } colType => colType.GetGenericArguments()[0],
                var x => x
            };

            var (value, valueType) = op switch
            {
                CompareOperator.In or CompareOperator.Nin => (
                    ParseValues(parseType, valueStrings),
                    typeof(IEnumerable<>).MakeGenericType(parseType)
                ),
                _ => (ParseValue(parseType, valueStrings.First()), parseType)
            };

            if(value is null)
                valueType = propSelector.ReturnType;

            var compareNodeType = typeof(PropertyComparesNode<>).MakeGenericType(valueType);
            return (IPropertyComparesNode)Activator.CreateInstance(compareNodeType, propSelector, op, value);
        }

        private object ParseValues(Type type, IEnumerable<string> valueStrings)
        {
            var values = valueStrings.Select(it => ParseValue(type, it));
            return typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(type).Invoke(null, new[] { values })!;
        }

        private object? ParseValue(Type type, string valueString)
        {
            string? str = valueString;
            if(str.Length == 0)
                str = null;

            var typeConverter = _valueParsers.FirstOrDefault(it => it.CanParse(type));
            if(typeConverter is null)
                throw new NotSupportedException($"Type {type} can not be convert from query");

            try
            {
                return typeConverter.Parse(str, type);
            }
            catch(Exception e)
            {
                throw new ArgumentException($"Can not parse {str ?? "<Empty>"} to type {type.Name}", e);
            }
        }

        private string FriendlyTypeDisplay(Type type)
        {
            return type switch
            {
                null => throw new ArgumentNullException(nameof(type)),
                { } when type == typeof(int) => "Integer32",
                { } when type == typeof(long) => "Integer64",
                { } when type == typeof(float) => "Float32",
                { } when type == typeof(double) => "Float64",
                { } when type.IsGenericParameter && type.GetGenericTypeDefinition() == typeof(Nullable<>) => FriendlyTypeDisplay(type.GetGenericArguments().First()) + "or Null",
                _ => type.Name,
            };
        }
    }
}
