using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonBooleanValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == "boolean";

        public void Validate(OpenApiSchema schema, JToken instance)
        {
            if (instance.Type != JTokenType.Boolean)
                throw new JsonValidationException(instance.Path, "Instance is not of type 'boolean'");
        }
    }
}