using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Net;
using System.Reflection;

namespace JwtAuth.Identity
{
    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isAuthorized = context.MethodInfo.GetCustomAttribute<AuthorisedAttribute>()?.Required ??
                             context.MethodInfo.DeclaringType?.GetCustomAttribute<AuthorisedAttribute>()
                                 ?.Required ?? false;

            if (isAuthorized)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            }
                        ] = []
                    }
                };
            }
            operation.Summary += context.MethodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
        }
    }
}
