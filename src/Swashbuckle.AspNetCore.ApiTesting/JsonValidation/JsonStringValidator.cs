using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonStringValidator : IJsonValidator
    {
        public bool CanValidate(OpenApiSchema schema) => schema.Type == "string";

        public void Validate(OpenApiSchema schema, JToken instance)
        {
            if (!new JTokenType[] { JTokenType.Date, JTokenType.Guid, JTokenType.String }.Contains(instance.Type))
                throw new JsonValidationException(instance.Path, "Instance is not of type 'string'");

            var stringValue = instance.Value<string>();

            // maxLength
            if (schema.MaxLength.HasValue && (stringValue.Length > schema.MaxLength.Value))
                throw new JsonValidationException(instance.Path, "String length is greater than maxLength");

            // minLength
            if (schema.MinLength.HasValue && (stringValue.Length < schema.MinLength.Value))
                throw new JsonValidationException(instance.Path, "String length is less than minLength");

            // pattern
            if ((schema.Pattern != null) && !Regex.IsMatch(stringValue, schema.Pattern))
                throw new JsonValidationException(instance.Path, "String does not match pattern");
        }
    }
}