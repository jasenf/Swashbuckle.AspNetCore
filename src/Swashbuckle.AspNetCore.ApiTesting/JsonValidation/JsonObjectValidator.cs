using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonObjectValidator : IJsonValidator
    {
        private readonly IJsonValidator _jsonValidator;

        public JsonObjectValidator(IJsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public bool CanValidate(OpenApiSchema schema) => schema.Type == "object";

        public void Validate(OpenApiSchema schema, JToken instance)
        {
            if (instance.Type != JTokenType.Object)
                throw new JsonValidationException(instance.Path, "Instance is not of type 'object'");

            var jObject = (JObject)instance;
            var properties = jObject.Properties();

            // maxProperties
            if (schema.MaxProperties.HasValue && properties.Count() > schema.MaxProperties.Value)
                throw new JsonValidationException(instance.Path, "Number of properties is greater than maxProperties");

            // minProperties
            if (schema.MinProperties.HasValue && properties.Count() < schema.MinProperties.Value)
                throw new JsonValidationException(instance.Path, "Number of properties is less than minProperties");

            // required
            if (schema.Required != null && schema.Required.Any(name => !jObject.ContainsKey(name)))
                throw new JsonValidationException(instance.Path, "Required property(s) not present");

            foreach (var property in properties)
            {
                // properties
                if (schema.Properties != null && schema.Properties.TryGetValue(property.Name, out OpenApiSchema propertySchema))
                {
                    _jsonValidator.Validate(propertySchema, property.Value);
                    continue;
                }

                if (!schema.AdditionalPropertiesAllowed)
                    throw new JsonValidationException(instance.Path, "Additional properties not allowed");

                // additionalProperties
                if (schema.AdditionalProperties != null)
                    _jsonValidator.Validate(schema.AdditionalProperties, property.Value);
            }
        }
    }
}