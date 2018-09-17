using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.AspNetCore.ApiTesting.Xunit
{
    [Collection("ApiTests")]
    public class ApiOperationFixture<TEntryPoint> :
        IClassFixture<WebApplicationFactory<TEntryPoint>> where TEntryPoint : class
    {
        private readonly ApiTestsHelperBase _apiTestsHelper;
        private readonly WebApplicationFactory<TEntryPoint> _webAppFactory;
        private string _documentName;
        private  string _pathTemplate;
        private  OperationType _operationType;

        public ApiOperationFixture(
            ApiTestsHelperBase apiTestsHelper,
            WebApplicationFactory<TEntryPoint> webAppFactory)
        {
            _apiTestsHelper = apiTestsHelper;
            _webAppFactory = webAppFactory;
        }

        public void Describe(
            string documentName,
            string pathTemplate,
            OperationType operationType,
            OpenApiOperation operationSpec)
        {
            _documentName = documentName;
            _pathTemplate = pathTemplate;
            _operationType = operationType;

            _apiTestsHelper.AddOperation(documentName, pathTemplate, operationType, operationSpec);
        }

        public async Task TestAsync(
            string expectedStatusCode,
            Dictionary<string, object> requestParameters = null,
            object requestBody = null)
        {
            await _apiTestsHelper.TestAsync(
                _documentName,
                _pathTemplate,
                _operationType,
                expectedStatusCode,
                requestParameters ?? new Dictionary<string, object>(),
                requestBody,
                _webAppFactory.CreateClient(),
                AssertResponseMatchesSpec);
        }

        public async Task TestAsync(string expectedStatusCode, Dictionary<string, object> requestParameters)
            => await TestAsync(expectedStatusCode, requestParameters);

        public async Task TestAsync(string expectedStatusCode, object requestBody)
            => await TestAsync(expectedStatusCode, null, requestBody);

        private void AssertResponseMatchesSpec(
            OpenApiDocument openApiDocument,
            string expectedStatusCode,
            OpenApiResponse responseSpec,
            HttpResponseMessage response)
        {
            Assert.Equal(expectedStatusCode, ((int)response.StatusCode).ToString());

            AssertResponseHeaders(openApiDocument, responseSpec, response);

            AssertResponseContent(openApiDocument, responseSpec, response);
        }

        private void AssertResponseHeaders(
            OpenApiDocument openApiDocument,
            OpenApiResponse responseSpec,
            HttpResponseMessage response)
        {
            var responseHeaders = response.Headers.Select(entry => entry.Key);

            foreach (var entry in responseSpec.Headers.Where(h => h.Value.Required))
            {
                Assert.Contains(entry.Key, responseHeaders);
            }
        }

        private void AssertResponseContent(
            OpenApiDocument openApiDocument,
            OpenApiResponse responseSpec,
            HttpResponseMessage response)
        {
            if (!responseSpec.Content.Any()) return;

            Assert.NotNull(response.Content);
            Assert.Contains(response.Content.Headers.ContentType.MediaType, responseSpec.Content.Keys);
            
            var mediaType = response.Content.Headers.ContentType.MediaType;
            if (!mediaType.Contains("json", StringComparison.InvariantCultureIgnoreCase)) return;

            var validator = new JsonValidator(openApiDocument);
            var validationException = Record.Exception(() =>
            {
                validator.Validate(
                    responseSpec.Content[mediaType].Schema,
                    JToken.Parse(response.Content.ReadAsStringAsync().Result));
            });

            Assert.Null(validationException?.Message);
        }
    }
}