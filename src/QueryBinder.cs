using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MQuery
{
    public class QueryBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = new ModelBindingQueryBuilder(bindingContext).Build();

            bindingContext.Result = ModelBindingResult.Success(query);
            return Task.CompletedTask;
        }
    }
}
