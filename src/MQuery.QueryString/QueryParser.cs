﻿using System;
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

    public class QueryParser<T>
    {
        private readonly string[]? _includeProps;

        public QueryParser(params string[] includeProperties)
        {
            if(includeProperties.Any())
                _includeProps = includeProperties;
        }

        private PropertyNode? ParseProperty(string key)
        {
            if(_includeProps?.Contains(key) == false)
                return null;

            var selectors = key.Split('.');
            var type = typeof(T);
            List<PropertyInfo> properties = new List<PropertyInfo>();
            foreach(var s in selectors)
            {
                var p = type.GetProperty(s, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(p is null)
                    return null;
                type = p.PropertyType;
                properties.Add(p);
            }
            return new PropertyNode(properties.First(), properties.Skip(1).ToArray());
        }

        public Query<T> Parse(string queryString)
        {
            if(queryString is null)
                throw new ArgumentNullException(nameof(queryString));

            if(queryString.StartsWith("?"))
                queryString = queryString[1..];

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

        public bool MatchSlicing(string key, string valueString, SlicingDocument document)
        {
            if(key == "$skip")
            {
                if(!int.TryParse(valueString, out var skip))
                    throw new ParseException($"$skip value must be integer") { Key = key };
                document.Skip = skip;
                return true;
            }

            if(key == "$limit")
            {
                if(!int.TryParse(valueString, out var limit))
                    throw new ParseException($"$limit value must be integer") { Key = key };
                document.Limit = limit;
                return true;
            }

            return false;
        }

        public bool MatchSort(string key, string valueString, SortDocument document)
        {
            var sortMatch = Regex.Match(key, @"^\$sort\[(\w+)\]");
            if(!sortMatch.Success)
                return false;

            var propName = sortMatch.Groups[1].Value;

            var prop = ParseProperty(propName);
            if(prop is null)
                return true;

            if(!int.TryParse(valueString, out int pattern) || !Enum.IsDefined(typeof(SortPattern), pattern))
                throw new ParseException("sort pattern can only be 1(asc) or -1(desc)") { Key = key, Values = new[] { valueString } };

            document.AddSortByProperty(prop, (SortPattern)pattern);
            return true;
        }

        public bool MatchFilter(string key, IEnumerable<string> valueStrings, FilterDocument document)
        {
            var filterMatch = Regex.Match(key, @"^([\w\.]+)(\[\$(.+?)\])?(\[\d*\])?$");
            if(!filterMatch.Success)
                return false;

            var propName = filterMatch.Groups[1].Value;
            var opString = filterMatch.Groups[3].Success ? filterMatch.Groups[3].Value : "eq";

            var prop = ParseProperty(propName);
            if(prop is null)
                return true;

            if(!Enum.TryParse<CompareOperator>(opString, true, out var op))
                return true;

            try
            {
                valueStrings = valueStrings.Where(it => it != null);
                var value = op == CompareOperator.In || op == CompareOperator.Nin
                    ? ConvertFromString(prop.PropertyType, valueStrings)
                    : ConvertFromString(prop.PropertyType, valueStrings.First());

                document.AddPropertyCompare(prop, new CompareNode(op, value));
                return true;
            }
            catch(ArgumentException e)
            {
                throw new ParseException(e.Message, e.InnerException) { Key = key, Values = valueStrings };
            }
        }

        private static object ConvertFromString(Type type, IEnumerable<string> valueStrings)
        {
            var values = valueStrings.Select(it => ConvertFromString(type, it));
            return typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(type).Invoke(null, new[] { values })!;
        }

        private static object? ConvertFromString(Type type, string valueString)
        {
            string? str = valueString;
            if(str.Length == 0)
                str = null;

            if(type == typeof(string))
                return str;

            var typeConverter = TypeDescriptor.GetConverter(type);
            try
            {
                return typeConverter.ConvertFromString(str);
            }
            catch(Exception e)
            {
                throw new ArgumentException($"can not convert {str ?? "<Empty>"} to type {type.Name}", nameof(valueString), e);
            }
        }

        public static IDictionary<string, string[]> StructureQueryString(string queryString)
        {
            return queryString.Split('&')
                              .Select(it =>
                              {
                                  var pair = it.Split('=');
                                  var key = HttpUtility.UrlDecode(pair.First());
                                  var value = pair.Length > 1 ? HttpUtility.UrlDecode(pair.Last()) : "";
                                  return (key, value);
                              })
                              .GroupBy(it => it.key, it => it.value)
                              .ToDictionary(it => it.Key, it => it.ToArray());
        }
    }
}
