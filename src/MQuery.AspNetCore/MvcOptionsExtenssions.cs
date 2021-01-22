using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MQuery.QueryString;

namespace MQuery.AspNetCore
{
    public static class MvcOptionsExtenssions
    {
        public static MvcOptions AddMQuery(this MvcOptions mvc, Action<ParserOptions> optionsBuilder = null)
        {
            var options = new ParserOptions();
            optionsBuilder?.Invoke(options);
            mvc.ModelBinderProviders.Insert(0, new QueryBinderProvider(options));
            return mvc;
        }
    }
}
