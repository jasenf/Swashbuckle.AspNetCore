using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonNullValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == "null";

        public void Validate(OpenApiSchema schema, JToken instance)
        {
            if (instance.Type != JTokenType.Null)
                throw new JsonValidationException(instance.Path, "Instance is not of type 'null'");
        }
    }
}