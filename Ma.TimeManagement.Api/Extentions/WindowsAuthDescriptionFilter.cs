using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ma.TimeManagement.Api.Extentions
{
    public class WindowsAuthDescriptionFilter : IOperationAsyncFilter
    {
        public async Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
        {
            if (context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any())
            {
                operation.Description +=
                    "<br><br>Supported authentication:<br>" +
                    "• <b>Windows Authentication</b> (automatic on domain)<br>" +
                    "• <b>JWT Bearer token</b> (use Authorize button above)";
            }
        }
    }

}
