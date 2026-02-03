//using Microsoft.OpenApi;
//using Swashbuckle.AspNetCore.SwaggerGen;

//public class UseActualParameterNameForBodyFilter : IOperationFilter
//{
//    public void Apply(OpenApiOperation operation, OperationFilterContext context)
//    {
//        if (operation.RequestBody?.Content == null)
//            return;

//        var bodyParam = context.MethodInfo.GetParameters()
//            .FirstOrDefault(p => p.GetCustomAttributes(true)
//                .Any(attr => attr is Microsoft.AspNetCore.Mvc.FromBodyAttribute));

//        if (bodyParam == null)
//            return;

//        var paramName = bodyParam.Name ?? "body";  // e.g. "item", "workItem"

//        foreach (var mediaType in operation.RequestBody.Content.Values)
//        {
//            // Pattern match to concrete type
//            if (mediaType.Schema is IOpenApiSchema concreteSchema)
//            {
//                if (concreteSchema.DynamicRef != null)
//                {
//                    // $ref exists → preserve reference, add title for better NSwag naming
//                    mediaType.Schema = new OpenApiSchema
//                    {
//                        Title = $"{bodyParam.ParameterType.Name} ({paramName})",
//                         // Preserve composition if present
//                        AllOf = concreteSchema.AllOf?.ToList(),
//                        OneOf = concreteSchema.OneOf?.ToList(),
//                        AnyOf = concreteSchema.AnyOf?.ToList()
//                    };
//                }
//                else
//                {
//                    // No reference → plain inline schema, just set title
//                    mediaType.Schema.isref = paramName;
//                }
//            }
//        }
//    }
//}