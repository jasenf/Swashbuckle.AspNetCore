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
    public class GetProductsTest : ApiOperationFixture<TestFirst.Startup>
    {
        public GetProductsTest(
            ApiTestsHelper apiTestsHelper,
            WebApplicationFactory<TestFirst.Startup> webAppFactory)
            : base(apiTestsHelper, webAppFactory)
        {
            Describe("v1", "/api/products", OperationType.Get, new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    [ "200" ] = new OpenApiResponse 
                    {
                        Description = "Retrieved products",
                        Content = new Dictionary<string, OpenApiMediaType >
                        {
                            [ "application/json" ] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "array",
                                    Items = new OpenApiSchema 
                                    {
                                        Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Product" }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task Returns200AndArrayOfProducts()
        {
            await TestAsync("200");
        }
    }
}