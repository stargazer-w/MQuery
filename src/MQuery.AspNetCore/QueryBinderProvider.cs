using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace MQuery.AspNetCore
{
    public class QueryBinderProvider : IModelBinderProvider
    {
        private readonly BinderOptions _options;

        public QueryBinderProvider(BinderOptions options)
        {
            _options = options;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if(context.Metadata.ModelType.IsGenericType && context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Query<>))
                return new QueryBinder(_options);

            return null;
        }
    }
}
