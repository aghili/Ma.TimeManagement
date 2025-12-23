//using Microsoft.AspNetCore.Authorization;
//using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace Ma.TimeManagement.Api.Extentions;

//public class SecurityRequirementsOperationFilter : IOperationFilter
//{
//    public void Apply(OpenApiOperation operation, OperationFilterContext context)
//    {
//        // Check if the method or controller has [Authorize]
//        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
//            .Union(context.MethodInfo.GetCustomAttributes(true))
//            .OfType<AuthorizeAttribute>().Any();

//        // Also check if [AllowAnonymous] is present to override authorization
//        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
//            .OfType<AllowAnonymousAttribute>().Any();

//        if (hasAuthorize && !hasAllowAnonymous)
//        {
//            operation.Security = new List<OpenApiSecurityRequirement>
//            {
//                new OpenApiSecurityRequirement
//                {
//                    {
//                        // Match the ID used in AddSecurityDefinition
//                        new OpenApiSecurityScheme {
//                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//                        },
//                        Array.Empty<string>()
//                    }
//                }
//            };
//        }
//    }
//}

