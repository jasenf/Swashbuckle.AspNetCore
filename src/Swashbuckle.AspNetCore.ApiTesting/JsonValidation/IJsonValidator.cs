using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public interface IJsonValidator
    {
        bool CanValidate(OpenApiSchema schema);

        void Validate(OpenApiSchema schema, JToken instance);
    }

    public class JsonValidationException : Exception
    {
        public JsonValidationException(string path, string errorMessage)
            : base($"Path: {path}. {errorMessage}") 
        {}
    }
}