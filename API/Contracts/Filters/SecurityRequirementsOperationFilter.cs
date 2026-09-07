using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    // 1. Bỏ dấu "/" ở cuối các endpoint để so sánh dễ hơn
    private readonly List<string> listPrefixesNeedProtect = new List<string> {
        "api/v1/users/me",
        "api/v1/users/update",
        "api/v1/users/delete-user",
        "api/v1/users/update-customer",
        "api/v1/chat",
        "api/v1/buildings",
        "api/v1/contacts",
        "api/v1/admin"
    };
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string? currentEndpoint = context.ApiDescription.RelativePath;
        if (currentEndpoint != null && listPrefixesNeedProtect.Any(prefix => currentEndpoint.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
