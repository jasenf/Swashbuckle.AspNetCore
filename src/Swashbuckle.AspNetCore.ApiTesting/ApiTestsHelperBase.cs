using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public abstract class ApiTestsHelperBase : IDisposable
    {
        private readonly Dictionary<string, OpenApiDocument> _openApiDocuments;
        private string _outputRoot; 

        public ApiTestsHelperBase()
        {
            _openApiDocuments = new Dictionary<string, OpenApiDocument>();
        }

        public void AddDocument(string documentName, OpenApiDocument openApiDocument)
        {
            _openApiDocuments.Add(documentName, openApiDocument);
        }

        public void OutputRoot(string outputRoot)
        {
            _outputRoot = outputRoot;
        }

        public void AddOperation(
            string documentName,
            string pathTemplate,
            OperationType operationType,
            OpenApiOperation operationMetadata)
        {
            var openApiDocument = GetDocument(documentName);

            if (!openApiDocument.Paths.TryGetValue(pathTemplate, out OpenApiPathItem pathMetadata))
            {
                pathMetadata = new OpenApiPathItem();
                openApiDocument.Paths[pathTemplate] = pathMetadata;
            }

            pathMetadata.Operations[operationType] = operationMetadata;
        }

        public async Task TestAsync(
            string documentName,
            string pathTemplate,
            OperationType operationType,
            string expectedStatusCode,
            Dictionary<string, object> requestParameters,
            object requestBody,
            HttpClient client,
            Action<OpenApiDocument, string, OpenApiResponse, HttpResponseMessage> assertAction)
        {
            var openApiDocument = GetDocument(documentName);

            var request = HttpRequestMessageFactory.CreateRequest(
                openApiDocument,
                pathTemplate,
                operationType,
                requestParameters ?? new Dictionary<string, object>(),
                requestBody);

            var response = await client.SendAsync(request);

            var responseSpec = openApiDocument.GetResponse(pathTemplate, operationType, expectedStatusCode);
            assertAction(openApiDocument, expectedStatusCode, responseSpec, response);
        }

        public void Dispose()
        {
            foreach (var entry in _openApiDocuments)
            {
                var outputDir = Path.Combine(_outputRoot, entry.Key);
                Directory.CreateDirectory(outputDir);

                using (var streamWriter = new StreamWriter(Path.Combine(outputDir, "openapi.json")))
                {
                    var openApiJsonWriter = new OpenApiJsonWriter(streamWriter);
                    entry.Value.SerializeAsV3(openApiJsonWriter);
                }
            }
        }

        private OpenApiDocument GetDocument(string documentName)
        {
            if (!_openApiDocuments.TryGetValue(documentName, out OpenApiDocument openApiDocument))
                throw new InvalidOperationException("TODO: Document not found");

            return openApiDocument;
        }
    }
}