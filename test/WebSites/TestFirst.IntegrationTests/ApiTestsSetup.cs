using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.ApiTesting;
using Xunit;

namespace TestFirst.IntegrationTests
{
    [CollectionDefinition("ApiTests")]
    public class ApiTestsCollection : ICollectionFixture<ApiTestsHelper>
    {}

    public class ApiTestsHelper : ApiTestsHelperBase
    {
        public ApiTestsHelper()
        {
            AddDocument("v1", new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "V1",
                    Title = "API V1"
                },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents
                {
                    Schemas = new Dictionary<string, OpenApiSchema>
                    {
                        [ "Product" ] = new OpenApiSchema 
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                [ "id" ] = new OpenApiSchema { Type = "number", ReadOnly = true },
                                [ "name" ] = new OpenApiSchema { Type = "string" },
                            },
                            Required = new SortedSet<string> { "id", "name" }
                        }
                    }
                } 
            });

            OutputRoot(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "TestFirst", "wwwroot", "api-docs"));
        }
    }
}