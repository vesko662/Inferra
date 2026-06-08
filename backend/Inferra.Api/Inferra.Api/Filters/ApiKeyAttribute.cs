using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inferra.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeader = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var expectedKey = configuration["MlService:ApiKey"];

            if (string.IsNullOrEmpty(expectedKey))
            {
                await next();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey)
                || providedKey != expectedKey)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API key." });
                return;
            }

            await next();
        }
    }
}
