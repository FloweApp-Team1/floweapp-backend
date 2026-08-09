using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel;

namespace IdentityService.Common.Swagger
{
    public class EnumParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var type = context.ParameterInfo?.ParameterType;
            if (type != null && type.IsEnum)
            {
                parameter.Schema.Type = "string";
                parameter.Schema.Enum = Enum.GetNames(type)
                    .Select(name => (IOpenApiAny)new OpenApiString(name))
                    .ToList();
            }
        }
    }
}

