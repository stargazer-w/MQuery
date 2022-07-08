using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MQuery.Expressions;
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
                    if(MatchFilter(pair.Key) is { IsMathed: true, Value: FilterKeyData data })
                    {
                        SetFilter(query.Document.Filter, data, pair.Value);
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
                catch(NotSupportedException)
                {
                    throw new ParseException("不支持的操作符") { Key = pair.Key, Values = pair.Value };
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

            var selector = match.Groups[1].Value;
            return MatchResult.Matched(new SortKeyData(selector));
        }

        public void SetSort(SortDocument<T> sort, string selectorString, string patternString)
        {
            if(!IsInvalidProp(selectorString))
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
            var match = Regex.Match(key, @"^([\w\.]+)(\[\$(.+?)\])*(\[\d*\])?$");
            if(!match.Success)
                return MatchResult.Unmatched<FilterKeyData>();

            var selector = match.Groups[1].Value;
            var ops = match.Groups[3].Captures.Cast<Capture>().Select(x => x.Value);

            return MatchResult.Matched(new FilterKeyData(selector, ops));
        }

        private void SetFilter(FilterDocument<T> filter, FilterKeyData data, IEnumerable<string> valueStrings)
        {
            if(!IsInvalidProp(data.Selector))
                return;

            var props = data.Selector
                .Split('.')
                .Select(x => char.ToUpper(x[0]) + x[1..])
                .ToArray();
            PropertySelector selector;
            try
            {
                selector = new PropertySelector(typeof(T), props);
            }
            catch
            {
                return;
            }

            filter.And(ParseOperation(selector, data.Ops.ToArray(), valueStrings));
        }

        private IParameterOperation ParseOperation(PropertySelector selector, string[] ops, IEnumerable<string> values)
        {
            var eleType = selector.PropertyCollectionElementType;
            return ops switch
            {
                [] => ParseOperation(selector, new[] { "eq" }, values),
                // 默认对集合类型的属性所有操作都改为对其任意元素，
                // 如array=x => array[$any]=x
                // 除非是array[$eq]=<null> 或者 array[$ne]=<null>
                _ when eleType is not null && !ops.Contains("any") && !(ops is ["eq"] or ["ne"] && values.First() is "")
                    => ParseOperation(selector, ops.Prepend("any").ToArray(), values),
                ["not", .. var other] => new Not(ParseOperation(selector, other, values)),
                ["any", .. var other] => eleType switch
                {
                    not null => new PropertyOperation(selector, new Any(ParseOperation(new PropertySelector(eleType), other, values))),
                    _ => throw new InvalidOperationException("The in operator must operate on an Array"),
                },
                ["nin"] => ParseOperation(selector, new[] { "not", "in" }, values),
                [var @operator] => new PropertyOperation(selector, parseOperator(@operator)),
                _ => throw new NotSupportedException()
            };

            IOperator parseOperator(string @operator)
            {
                return @operator switch
                {
                    "eq" => createOperator(typeof(Equal<>), parseValue()),
                    "ne" => createOperator(typeof(NotEqual<>), parseValue()),
                    "gt" => createOperator(typeof(GreaterThen<>), parseValue()),
                    "gte" => createOperator(typeof(GreaterThenOrEqual<>), parseValue()),
                    "lt" => createOperator(typeof(LessThen<>), parseValue()),
                    "lte" => createOperator(typeof(LessThenOrEqual<>), parseValue()),
                    "in" => createOperator(typeof(In<>), parseValues()),
                    _ => throw new NotSupportedException()
                };

                IOperator createOperator(Type opType, object? value) => (IOperator)Activator.CreateInstance(opType.MakeGenericType(selector.PropertyType), value);
                object? parseValue() => ParseValue(selector.PropertyType, values.First());
                object? parseValues() => ParseValues(selector.PropertyType, values);
            }
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

        private bool IsInvalidProp(string selector)
        {
            if(_includeProps == null)
                return true;

            return _includeProps.Any(selector.StartsWith);
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
