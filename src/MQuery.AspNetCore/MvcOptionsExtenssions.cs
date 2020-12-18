using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace MQuery.AspNetCore
{
    public static class MvcOptionsExtenssions
    {
        public static MvcOptions AddMQuery(this MvcOptions mvc, Action<BinderOptions> optionsBuilder = null)
        {
            var options = new BinderOptions();
            optionsBuilder?.Invoke(options);
            mvc.ModelBinderProviders.Insert(0, new QueryBinderProvider(options));
            return mvc;
        }
    }
}
