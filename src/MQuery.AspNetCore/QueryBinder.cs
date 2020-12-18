using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using MQuery.QueryString;

namespace MQuery.AspNetCore
{
    public class QueryBinder : IModelBinder
    {
        private readonly BinderOptions _options;

        public QueryBinder(BinderOptions options)
        {
            _options = options;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var queryType = bindingContext.ModelMetadata.ModelType;
            var elementType = queryType.GenericTypeArguments.First();
            var queryString = bindingContext.HttpContext.Request.QueryString;

            var parserType = typeof(QueryParser<>).MakeGenericType(elementType);
            string[] include = Array.Empty<string>();
            if (
                bindingContext.ModelMetadata is DefaultModelMetadata meta
                && meta.Attributes.ParameterAttributes.FirstOrDefault(a => a is BindAttribute) is BindAttribute bind
            )
            {
                include = bind.Include;
            }
            var parser = Activator.CreateInstance(parserType, include);

            try
            {
                var query = parserType.GetMethod("Parse").Invoke(parser, new object[] { queryString.ToString() });
                var document = queryType.GetProperty("Document").GetValue(query) as QueryDocument;

                if(document.Slicing.Limit == null)
                    document.Slicing.Limit = _options.DefaultLimit;

                if(document.Slicing.Limit > _options.MaxLimit)
                    document.Slicing.Limit = _options.MaxLimit.Value;

                bindingContext.Result = ModelBindingResult.Success(query);
            }
            catch (ParseException e)
            {
                bindingContext.ModelState.AddModelError(e.Key, e.Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }

    public class BinderOptions
    {
        public int DefaultLimit { get; set; } = 1;

        public int? MaxLimit { get; set; }
    }
}
