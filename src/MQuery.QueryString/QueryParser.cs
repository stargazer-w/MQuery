using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                queryString = queryString[1..];

            var query = new Query<T>();
            var parameters = Utils.StructureQueryString(queryString);
            var props = typeof(T).GetProperties();
            foreach(var pair in parameters.Where(it => !string.IsNullOrEmpty(it.Key)))
            {
                try
                {
                    if(MatchFilter(pair.Key) is { IsMathed: true, Value: FilterKeyData filterKeyData })
                    {
                        SetFilter(query.Document.Filter, filterKeyData, pair.Value);
                    }
                    else if(MatchSort(pair.Key) is { IsMathed: true, Value: SortKeyData sortKeyData })
                    {
                        SetSort(query.Document.Sort, sortKeyData, pair.Value.First());
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

        private void SetSort(SortDocument<T> sort, SortKeyData data, string patternString)
        {
            if(!IsInvalidProp(data.Selector))
                return;

            if(!TryParsePropertySelector(data.Selector, out var selector))
                return;

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
            sort.SortBys.Add(new(selector!, pattern));
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

            if(!TryParsePropertySelector(data.Selector, out var selector))
                return;

            var ops = Unsugar(selector, data.Ops.ToArray(), valueStrings);
            filter.Operation = ParseOperation(filter.Operation, selector, ops, valueStrings);
        }

        public string[] Unsugar<U>(PropertySelector<U> selector, string[] ops, IEnumerable<string> values)
        {
            var operators = new[] { "eq", "ne", "gt", "gte", "lt", "lte", "in", };
            IEnumerable<string> unsugaredOps = ops;

            // nin 是 not in的简称
            if(unsugaredOps.LastOrDefault() is "nin")
            {
                unsugaredOps = unsugaredOps
                    .Take(unsugaredOps.Count() - 1)
                    .Append("not")
                    .Append("in");
            }

            // eq操作符是默认操作符
            if(!operators.Contains(unsugaredOps.LastOrDefault()))
            {
                unsugaredOps = unsugaredOps.Append("eq");
            }

            // 对于集合属性，默认对其执行Any
            // 除非是array eq null 或 array ne null
            if(
                selector.PropertyCollectionElementType is not null
                && !unsugaredOps.Contains("any")
                && !(unsugaredOps.Last() is "eq" or "ne" && values.First() == "")
            )
            {
                unsugaredOps = unsugaredOps.Prepend("any");
            }

            return unsugaredOps.ToArray();
        }

        IParameterOperation ParseOperation<U>(IParameterOperation? operation, PropertySelector<U> selector, string[] ops, IEnumerable<string> values)
        {
            var op = new PropertyOperation<U>(selector, ParseOperator(selector, ops, values));
            switch(operation)
            {
                case null:
                    return op;
                case And and:
                    and.Operators.Add(op);
                    return and;
                default:
                    return new And(operation, op);
            }
        }

        IOperator ParseOperator<U>(PropertySelector<U> selector, string[] ops, IEnumerable<string> values)
        {
            return ops switch
            {
                [var @operator] => parseOperator(@operator),
                ["not", .. var others] => new Not(ParseOperator(selector, others, values)),
                ["any", .. var eleOps] => selector.PropertyCollectionElementType switch
                {
                    Type eleType => ParseAny(eleType, eleOps, values),
                    _ => throw new NotSupportedException(),
                },
                _ => throw new NotSupportedException()
            };

            IOperator parseOperator(string @operator) => ParseSimpleOperator(@operator, selector.PropertyType, values);
        }

        Any ParseAny(Type eleType, string[] eleOps, IEnumerable<string> values)
        {
            var func = parseOperation<object>;
            var op = (IParameterOperation)func.Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(eleType)
                .Invoke(func.Target, Array.Empty<object>())!;
            return new Any(op);

            IParameterOperation parseOperation<U>()
            {
                return ParseOperation<U>(null, new(), eleOps, values);
            }
        }

        IOperator ParseSimpleOperator(string @operator, Type type, IEnumerable<string> values)
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

            object? parseValue() => ParseValue(type, values.First());
            object? parseValues() => ParseValues(type, values);
            IOperator createOperator(Type opType, object? value) => (IOperator)Activator.CreateInstance(opType.MakeGenericType(type), value)!;
        }

        IEnumerable ParseValues(Type type, IEnumerable<string> valueStrings)
        {
            var values = valueStrings.Select(it => ParseValue(type, it));
            return (IEnumerable)typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(type).Invoke(null, new[] { values })!;
        }

        object? ParseValue(Type type, string valueString)
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

        private bool TryParsePropertySelector(string stringSelector, [NotNullWhen(true)] out PropertySelector<T>? selector)
        {
            var props = stringSelector
                .Split('.')
                .Select(x => char.ToUpper(x[0]) + x[1..])
                .ToArray();
            try
            {
                selector = new PropertySelector<T>(props);
            }
            catch
            {
                selector = null;
                return false;
            }

            return true;
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
