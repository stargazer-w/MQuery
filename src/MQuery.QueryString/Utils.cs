using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

[assembly: InternalsVisibleTo("MQuery.QueryString.Tests")]

namespace MQuery.QueryString
{
    internal class Utils
    {
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
