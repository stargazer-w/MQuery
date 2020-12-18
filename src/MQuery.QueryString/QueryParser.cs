using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using MQuery.Filter;
using MQuery.Sort;

namespace MQuery.QueryString
{

    public class QueryParser
    {
        public static Query<T> Parse<T>(string queryString)
        {
            if (queryString is null)
                throw new ArgumentNullException(nameof(queryString));

            var query = new Query<T>();
            var parameters = StructureQueryString(queryString);
            var props = typeof(T).GetProperties();
            foreach (var pair in parameters.Where(it => !string.IsNullOrEmpty(it.Key)))
            {
                try
                {
                    var (property, compare) = ParseFilter(props, pair.Key, pair.Value);
                    query.Document.Filter.AddPropertyCompare(property, compare);
                    continue;
                }
                catch (ArgumentException)
                {
                }


                try
                {
                    var (property, sortPatten) = ParseSort(props, pair.Key, pair.Value.First());
                    query.Document.Sort.AddSortByProperty(property, sortPatten);
                    continue;
                }
                catch (ArgumentException)
                {
                }

                if (pair.Key == "$skip")
                {
                    if (!int.TryParse(pair.Value.First(), out var skip))
                        throw new ParseException($"$skip value must be integer", pair.Key);
                    query.Document.Slicing.Skip = skip;
                    continue;
                }

                if (pair.Key == "$limit")
                {
                    if (!int.TryParse(pair.Value.First(), out var limit))
                        throw new ParseException($"$limit value must be integer", pair.Key);
                    query.Document.Slicing.Limit = limit;
                    continue;
                }
            }
            return query;
        }

        public static (PropertyNode property, SortPattern sortPattern) ParseSort(
            IEnumerable<PropertyInfo> propertyInfos,
            string key,
            string valueString
        )
        {
            var sortMatch = Regex.Match(key, @"^\$sort\[(\w+)\]");
            if (!sortMatch.Success)
                throw new ArgumentException("invalid key", nameof(key));

            var propName = sortMatch.Groups[1].Value;

            var prop = propertyInfos.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
            if (prop is null)
                throw new ParseException($"property {propName} is not define", key);

            if (!int.TryParse(valueString, out int pattern) || !Enum.IsDefined(typeof(SortPattern), pattern))
                throw new ParseException("sort pattern can only be 1(asc) or -1(desc)", key) { Values = new[] { valueString } };

            return (new PropertyNode(prop), (SortPattern)pattern);
        }

        public static (PropertyNode property, CompareNode compare) ParseFilter(
            IEnumerable<PropertyInfo> propertyInfos,
            string key,
            IEnumerable<string> valueStrings
        )
        {
            var filterMatch = Regex.Match(key, @"^(\w+)(\[\$(.+)\])?(\[\d*\])?$", RegexOptions.Compiled);
            if (!filterMatch.Success)
                throw new ArgumentException("invalid key", nameof(key));

            var propName = filterMatch.Groups[1].Value;
            var opString = filterMatch.Groups[3].Success ? filterMatch.Groups[3].Value : "eq";

            var prop = propertyInfos.FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
            if (prop is null)
                throw new ParseException($"property {propName} is not define", key);

            if (!Enum.TryParse<CompareOperator>(opString, true, out var op))
                throw new ParseException("invalid operator", key);

            try
            {
                valueStrings = valueStrings.Where(it => it != null);
                var value = op == CompareOperator.In || op == CompareOperator.Nin
                    ? valueStrings.Select(it => ConvertFromString(prop.PropertyType, it))
                    : ConvertFromString(prop.PropertyType, valueStrings.First());

                return (new PropertyNode(prop), new CompareNode(op, value));
            }
            catch (ArgumentException e)
            {
                throw new ParseException(e.Message, key, e.InnerException) { Values = valueStrings };
            }
        }

        private static object ConvertFromString(Type type, string valueString)
        {
            if (valueString.Length == 0)
                valueString = null;

            if (type == typeof(string))
                return valueString;

            var typeConverter = TypeDescriptor.GetConverter(type);
            try
            {
                return typeConverter.ConvertFromString(valueString);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"can not convert {valueString ?? "<Empty>"} to type {type.Name}", nameof(valueString), e);
            }
        }

        public static IDictionary<string, string[]> StructureQueryString(string queryString)
        {
            return queryString.Split('&')
                              .Select(it =>
                              {
                                  var pair = it.Split('=');
                                  var key = HttpUtility.UrlDecode(pair.First());
                                  var value = pair.Length > 1 ? HttpUtility.UrlDecode(pair.Last()) : null;
                                  return (key, value);
                              })
                              .GroupBy(it => it.key)
                              .Select(it => (it.Key, Value: it.Select(p => p.value)))
                              .Aggregate(
                                  new Dictionary<string, string[]>(),
                                  (dict, pair) =>
                                  {
                                      dict.Add(pair.Key, pair.Value.ToArray());
                                      return dict;
                                  }
                              );
        }
    }
}
