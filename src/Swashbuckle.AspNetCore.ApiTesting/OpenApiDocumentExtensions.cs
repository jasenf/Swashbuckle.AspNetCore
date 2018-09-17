using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class OpenApiDocumentExtensions
    {
        public static OpenApiPathItem GetPathItem(
            this OpenApiDocument openApiDocument,
            string pathTemplate)
        {
            if (!openApiDocument.Paths.TryGetValue(pathTemplate, out OpenApiPathItem pathItem))
                throw new InvalidOperationException("TODO: PathItem not found");

            return pathItem;
        }

        public static OpenApiOperation GetOperation(
            this OpenApiDocument openApiDocument,
            string pathTemplate,
            OperationType operationType)
        {
            var pathItem = openApiDocument.GetPathItem(pathTemplate);

            if (!pathItem.Operations.TryGetValue(operationType, out OpenApiOperation operation))
                throw new InvalidOperationException("TODO: Operation not found");

            return operation;
        }

        public static OpenApiResponse GetResponse(
            this OpenApiDocument openApiDocument,
            string pathTemplate,
            OperationType operationType,
            string responseCode)
        {
            var operation = openApiDocument.GetOperation(pathTemplate, operationType);

            if (!operation.Responses.TryGetValue(responseCode, out OpenApiResponse response))
                throw new InvalidOperationException("TODO: Response not found");

            return response;
        }
    }
}