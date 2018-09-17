using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class JsonValidatorTests
    {
        [Theory]
        [InlineData("null", "{}", "Path: . Instance is not of type 'null'")]
        [InlineData("null", "null", null)]
        [InlineData("boolean", "'foobar'", "Path: . Instance is not of type 'boolean'")]
        [InlineData("boolean", "true", null)]
        [InlineData("object", "'foobar'", "Path: . Instance is not of type 'object'")]
        [InlineData("object", "{}", null)]
        [InlineData("array", "'foobar'", "Path: . Instance is not of type 'array'")]
        [InlineData("array", "[]", null)]
        [InlineData("number", "'foobar'", "Path: . Instance is not of type 'number'")]
        [InlineData("number", "1", null)]
        [InlineData("string", "{}", "Path: . Instance is not of type 'string'")]
        [InlineData("string", "'foobar'", null)]
        public void Validate_ThrowsException_IfInstanceNotOfExpectedType(
            string schemaType,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema { Type = schemaType };
            var instance = JToken.Parse(instanceText);

            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(5.0, "9", "Path: . Number is not evenly divisible by multipleOf")]
        [InlineData(5.0, "10", null)]
        public void Validate_ThrowsException_IfNumberNotEvenlyDivisibleByMultipleOf(
            decimal schemaMultipleOf,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema { Type = "number", MultipleOf = schemaMultipleOf };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(10.0, "10.1", "Path: . Number is greater than maximum")]
        [InlineData(10.0, "10.0", null)]
        public void Validate_ThrowsException_IfNumberGreaterThanMaximum(
            decimal schemaMaximum,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema { Type = "number", Maximum = schemaMaximum };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(10.0, "10.0", "Path: . Number is greater than, or equal to, maximum")]
        [InlineData(10.0, "9.9", null)]
        public void Validate_ThrowsException_IfNumberGreaterThanOrEqualToMaximumAndExclusiveMaximumSet(
            decimal schemaMaximum,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "number",
                Maximum = schemaMaximum,
                ExclusiveMaximum = true
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(10.0, "9.9", "Path: . Number is less than minimum")]
        [InlineData(10.0, "10.0", null)]
        public void Validate_ThrowsException_IfNumberLessThanMinimum(
            decimal schemaMinimum,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema { Type = "number", Minimum = schemaMinimum };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(10.0, "10.0", "Path: . Number is less than, or equal to, minimum")]
        [InlineData(10.0, "10.1", null)]
        public void Validate_ThrowsException_IfNumberLessThanOrEqualToMinimumAndExclusiveMinimumSet(
            decimal schemaMinimum,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "number",
                Minimum = schemaMinimum,
                ExclusiveMinimum = true
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(5, "'123456'", "Path: . String length is greater than maxLength")]
        [InlineData(5, "'12345'", null)]
        public void Validate_ThrowsException_IfStringLengthGreaterThanMaxLength(
            int schemaMaxLength,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "string",
                MaxLength = schemaMaxLength
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(5, "'1234'", "Path: . String length is less than minLength" )]
        [InlineData(5, "'12345'", null)]
        public void Validate_ThrowsException_IfStringLengthLessThanMinLength(
            int schemaMinLength,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "string",
                MinLength = schemaMinLength
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("^[a-z]{3}$", "'aa1'", "Path: . String does not match pattern")]
        [InlineData("^[a-z]{3}$", "'aaz'", null)]
        public void Validate_ThrowsException_IfStringDoesNotMatchPattern(
            string schemaPattern,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "string",
                Pattern = schemaPattern
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("boolean", "[ true, 'foo' ]", "Path: [1]. Instance is not of type 'boolean'")]
        [InlineData("number", "[ 123, 'foo' ]", "Path: [1]. Instance is not of type 'number'")]
        [InlineData("boolean", "[ true, false ]", null)]
        public void Validate_ThrowsException_IfArrayItemDoesNotMatchItemsSchema(
            string itemsSchemaType,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "array",
                Items = new OpenApiSchema { Type = itemsSchemaType }
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(2, "[ 1, 2, 3 ]", "Path: . Array size is greater than maxItems")]
        [InlineData(2, "[ 1, 2 ]", null)]
        public void Validate_ThrowsException_IfArraySizeGreaterThanMaxItems(
            int schemaMaxItems,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "array",
                MaxItems = schemaMaxItems
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(2, "[ 1 ]", "Path: . Array size is less than minItems")]
        [InlineData(2, "[ 1, 2 ]", null)]
        public void Validate_ThrowsException_IfArraySizeLessThanMinItems(
            int schemaMinItems,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "array",
                MinItems = schemaMinItems
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(2, "[ 1, 1, 3 ]", "Path: . Array does not contain uniqueItems")]
        [InlineData(2, "[ 1, 2, 3 ]", null)]
        public void Validate_ThrowsException_IfArrayDoesNotContainUniqueItemsAndUniqueItemsSet(
            int schemaMinItems,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "array",
                UniqueItems = true
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(1, "{ \"id\": 1, \"name\": \"foo\" }", "Path: . Number of properties is greater than maxProperties")]
        [InlineData(1, "{ \"id\": 1 }", null)]
        public void Validate_ThrowsException_IfNumberOfPropertiesGreaterThanMaxProperties(
            int schemaMaxProperties,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                MaxProperties = schemaMaxProperties
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(2, "{ \"id\": 1 }", "Path: . Number of properties is less than minProperties")]
        [InlineData(2, "{ \"id\": 1, \"name\": \"foo\" }", null)]
        public void Validate_ThrowsException_IfNumberOfPropertiesLessThanMinProperties(
            int schemaMinProperties,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                MinProperties = schemaMinProperties
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(new[] { "id", "name" }, "{ \"id\": 1 }", "Path: . Required property(s) not present")]
        [InlineData(new[] { "id", "name" }, "{ \"id\": 1, \"name\": \"foo\" }", null)]
        public void Validate_ThrowsException_IfRequiredPropertyNotPresent(
            string[] schemaRequired,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                Required = new SortedSet<string>(schemaRequired)
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("number", "{ \"id\": \"foo\" }", "Path: id. Instance is not of type 'number'")]
        [InlineData("string", "{ \"id\": 123 }", "Path: id. Instance is not of type 'string'")]
        [InlineData("number", "{ \"id\": 123 }", null)]
        public void Validate_ThrowsException_IfKnownPropertyDoesNotMatchPropertySchema(
            string propertySchemaType,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    [ "id" ] = new OpenApiSchema { Type = propertySchemaType }
                }
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("number", "{ \"id\": \"foo\" }", "Path: id. Instance is not of type 'number'")]
        [InlineData("string", "{ \"name\": 123 }", "Path: name. Instance is not of type 'string'")]
        [InlineData("number", "{ \"description\": 123 }", null)]
        public void Validate_ThrowsException_IfAdditionalPropertyDoesNotMatchAdditionalPropertiesSchema(
            string additionalPropertiesType,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                AdditionalProperties = new OpenApiSchema { Type = additionalPropertiesType }
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData(false, "{ \"id\": \"foo\" }", "Path: . Additional properties not allowed")]
        [InlineData(true, "{ \"id\": \"foo\" }", null)]
        public void Validate_ThrowsException_IfAdditionalPropertiesPresentAndAdditionalPropertiesAllowedUnset(
            bool additionalPropertiesAllowed,
            string instanceText,
            string expectedExceptionMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = additionalPropertiesAllowed
            };
            var instance = JToken.Parse(instanceText);
            
            var exception = Record.Exception(() => Subject().Validate(openApiSchema, instance));

            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        private JsonValidator Subject()
        {
            return new JsonValidator(new OpenApiDocument());
        }
    }
}
