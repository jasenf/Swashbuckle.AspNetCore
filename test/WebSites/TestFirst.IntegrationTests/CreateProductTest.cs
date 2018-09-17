using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.ApiTesting.Xunit;

namespace TestFirst.IntegrationTests
{
    public class CreateProductTest : ApiOperationFixture<TestFirst.Startup>
    {
        public CreateProductTest(
            ApiTestsHelper apiTestsHelper,
            WebApplicationFactory<TestFirst.Startup> webAppFactory)
            : base(apiTestsHelper, webAppFactory)
        {
            Describe("v1", "/api/products", OperationType.Post, new OpenApiOperation
            {
                RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType> 
                    {
                        [ "application/json" ] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Product" }
                            }
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    [ "201" ] = new OpenApiResponse 
                    {
                        Description = "Product created",
                        Headers = new Dictionary<string, OpenApiHeader>
                        {
                            [ "Location" ] = new OpenApiHeader
                            {
                                Description = "The URI of the created product",
                                Required = true,
                                Schema = new OpenApiSchema { Type = "string" }
                            }
                        }
                    },
                    [ "400" ] = new OpenApiResponse 
                    {
                        Description = "Invalid request",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [ "application/json" ] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    AdditionalProperties = new OpenApiSchema
                                    {
                                        Type = "array",
                                        Items = new OpenApiSchema { Type = "string" }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task Returns201AndLocationHeader_GivenAValidRequestBody()
        {
            await TestAsync("201",
                requestBody: new
                {
                    name = "Test product"
                }
            );
        }

        [Fact]
        public async Task Returns400AndErrorMap_GivenAnInvalidRequestBody()
        {
            await TestAsync("400",
                requestBody: new
                { }
            );
        }
    }
}