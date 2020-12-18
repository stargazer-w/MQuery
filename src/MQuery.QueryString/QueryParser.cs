using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using MQuery.Filter;
using MQuery.Slicing;
using MQuery.Sort;

namespace MQuery.QueryString
{

    public class QueryParser
    {
        public static Query<T> Parse<T>(string queryString)
        {
            if(queryString is null)
                throw new ArgumentNullException(nameof(queryString));

            var query = new Query<T>();
            var parameters = StructureQueryString(queryString);
            var props = typeof(T).GetProperties();
            foreach(var pair in parameters.Where(it => !string.IsNullOrEmpty(it.Key)))
            {
                if(MatchFilter(pair.Key, pair.Value, query.Document.Filter))
                    continue;

                if(MatchSort(pair.Key, pair.Value.First(), query.Document.Sort))
                    continue;

                if(MatchSlicing(pair.Key, pair.Value.First(), query.Document.Slicing))
                    continue;
            }
            return query;
        }

        public static bool MatchSlicing(string key, string valueString, SlicingDocument document)
        {
            if(key == "$skip")
            {
                if(!int.TryParse(valueString, out var skip))
                    throw new ParseException($"$skip value must be integer", key);
                document.Skip = skip;
                return true;
            }

            if(key == "$limit")
            {
                if(!int.TryParse(valueString, out var limit))
                    throw new ParseException($"$limit value must be integer", key);
                document.Limit = limit;
                return true;
            }

            return false;
        }

        public static bool MatchSort(string key, string valueString, SortDocument document)
        {
            var sortMatch = Regex.Match(key, @"^\$sort\[(\w+)\]");
            if(!sortMatch.Success)
                return false;

            var propName = sortMatch.Groups[1].Value;

            var prop = document.ElementType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(prop is null)
                return true;

            if(!int.TryParse(valueString, out int pattern) || !Enum.IsDefined(typeof(SortPattern), pattern))
                throw new ParseException("sort pattern can only be 1(asc) or -1(desc)", key) { Values = new[] { valueString } };

            document.AddSortByProperty(new PropertyNode(prop), (SortPattern)pattern);
            return true;
        }

        public static bool MatchFilter(string key, IEnumerable<string> valueStrings, FilterDocument document)
        {
            var filterMatch = Regex.Match(key, @"^(\w+)(\[\$(.+)\])?(\[\d*\])?$", RegexOptions.Compiled);
            if(!filterMatch.Success)
                return false;

            var propName = filterMatch.Groups[1].Value;
            var opString = filterMatch.Groups[3].Success ? filterMatch.Groups[3].Value : "eq";

            var prop = document.ElementType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if(prop is null)
                return true;

            if(!Enum.TryParse<CompareOperator>(opString, true, out var op))
                return true;

            try
            {
                valueStrings = valueStrings.Where(it => it != null);
                var value = op == CompareOperator.In || op == CompareOperator.Nin
                    ? valueStrings.Select(it => ConvertFromString(prop.PropertyType, it))
                    : ConvertFromString(prop.PropertyType, valueStrings.First());

                document.AddPropertyCompare(new PropertyNode(prop), new CompareNode(op, value));
                return true;
            }
            catch(ArgumentException e)
            {
                throw new ParseException(e.Message, key, e.InnerException) { Values = valueStrings };
            }
        }

        private static object ConvertFromString(Type type, string valueString)
        {
            if(valueString.Length == 0)
                valueString = null;

            if(type == typeof(string))
                return valueString;

            var typeConverter = TypeDescriptor.GetConverter(type);
            try
            {
                return typeConverter.ConvertFromString(valueString);
            }
            catch(Exception e)
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
