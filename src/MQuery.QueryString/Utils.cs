using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web;

[assembly: InternalsVisibleTo("MQuery.QueryString.Tests")]

namespace MQuery.QueryString
{
    internal class Utils
    {
        public static LambdaExpression StringToPropSelector<T>(string selectorString)
        {
            var singleSelectors = selectorString.Split('.');
            if(singleSelectors.Length == 0)
                throw new ArgumentException(nameof(selectorString));

            Expression<Func<T, T>> self = x => x;
            var body = self.Body;
            foreach(var propName in singleSelectors)
            {
                body = Expression.Property(body, propName);
            }
            return Expression.Lambda(body, self.Parameters);
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
