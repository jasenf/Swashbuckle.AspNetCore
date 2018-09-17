using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class JsonArrayValidator : IJsonValidator
    {
        private readonly IJsonValidator _jsonValidator;

        public JsonArrayValidator(IJsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public bool CanValidate(OpenApiSchema schema) => schema.Type == "array";

        public void Validate(OpenApiSchema schema, JToken instance)
        {
            if (instance.Type != JTokenType.Array)
                throw new JsonValidationException(instance.Path, "Instance is not of type 'array'");

            var arrayInstance = (JArray)instance;

            // items
            if (schema.Items != null)
            {
                foreach (var itemInstance in arrayInstance)
                {
                    _jsonValidator.Validate(schema.Items, itemInstance);
                }
            }

            // maxItems
            if (schema.MaxItems.HasValue && (arrayInstance.Count() > schema.MaxItems.Value))
                throw new JsonValidationException(instance.Path, "Array size is greater than maxItems");

            // minItems
            if (schema.MinItems.HasValue && (arrayInstance.Count() < schema.MinItems.Value))
                throw new JsonValidationException(instance.Path, "Array size is less than minItems");

            // uniqueItems
            if (schema.UniqueItems.HasValue && (arrayInstance.Count() != arrayInstance.Distinct().Count()))
                throw new JsonValidationException(instance.Path, "Array does not contain uniqueItems");
        }
    }
}