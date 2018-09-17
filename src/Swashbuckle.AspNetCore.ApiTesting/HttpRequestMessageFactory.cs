using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class HttpRequestMessageFactory
    {
        public static HttpRequestMessage CreateRequest(
            OpenApiDocument openApiDocument,
            string pathTemplate,
            OperationType operationType,
            Dictionary<string, object> requestParameters,
            object requestBody = null)
        {
            var pathSpec = openApiDocument.GetPathItem(pathTemplate);
            var operationSpec = openApiDocument.GetOperation(pathTemplate, operationType);
            var parameterSpecs = ExpandParametersMetadata(openApiDocument, pathSpec, operationSpec);

            var request = new HttpRequestMessage()
            {
                RequestUri = CreateRequestUri(pathTemplate, parameterSpecs, requestParameters),
                Method = HttpMethodMap[operationType]
            };

            foreach (var entry in CreateHeaders(operationSpec, parameterSpecs, requestParameters))
            {
                request.Headers.Add(entry.Key, entry.Value);
            }

            if (operationSpec.RequestBody != null && requestBody != null)
                request.Content = CreateContent(operationSpec.RequestBody, requestParameters, requestBody);

            return request;
        }

        private static IEnumerable<OpenApiParameter> ExpandParametersMetadata(
            OpenApiDocument openApiDocument,
            OpenApiPathItem pathSpec,
            OpenApiOperation operationSpec)
        {
            var securityParameters = DeriveSecurityParameters(openApiDocument, operationSpec);

            return securityParameters
                .Concat(pathSpec.Parameters)
                .Concat(operationSpec.Parameters)
                .Distinct(new OpenApiParameterComparer());
        }

        private static IEnumerable<OpenApiParameter> DeriveSecurityParameters(
            OpenApiDocument openApiDocument,
            OpenApiOperation operationMetadata)
        {
            // TODO
            return new OpenApiParameter[]{};
        }

        private static Uri CreateRequestUri(
            string pathTemplate,
            IEnumerable<OpenApiParameter> parameterSpecs,
            Dictionary<string, object> requestParameters = null)
        {
            var uriStringBuilder = new StringBuilder(pathTemplate);

            foreach (var parameterSpec in parameterSpecs)
            {
                if (parameterSpec.In != ParameterLocation.Path) continue;

                if (requestParameters.TryGetValue(parameterSpec.Name, out object value))
                    uriStringBuilder.Replace($"{{{parameterSpec.Name}}}", value.ToString());
            }

            return new Uri(uriStringBuilder.ToString(), UriKind.Relative);
        }

        private static Dictionary<string, IEnumerable<string>> CreateHeaders(
            OpenApiOperation operationSpec,
            IEnumerable<OpenApiParameter> parameterSpecs,
            Dictionary<string, object> requestParameters = null)
        {
            var headers = new Dictionary<string, IEnumerable<string>>();

            headers.Add("Accept", operationSpec.Responses.Values
                .SelectMany(r => r.Content.Keys)
                .Distinct());

            foreach (var parameterSpec in parameterSpecs)
            {
                if (parameterSpec.In != ParameterLocation.Header) continue;

                if (requestParameters.TryGetValue(parameterSpec.Name, out object value))
                    headers.Add(parameterSpec.Name, (IEnumerable<string>)value);
            }

            return headers; 
        }

        private static HttpContent CreateContent(
            OpenApiRequestBody requestBodyMetadata,
            Dictionary<string, object> requestParameters,
            object requestBody)
        {
            var mediaType = requestParameters.ContainsKey("Content-Type")
                ? requestParameters["ContentType"].ToString()
                : requestBodyMetadata.Content.Keys.FirstOrDefault();

            var mediaTypeFormatter = SupportedMediaTypeFormatters.FindWriter(
                typeof(object),
                new MediaTypeHeaderValue(mediaType));

            return new ObjectContent(typeof(object), requestBody, mediaTypeFormatter);
        }

        private static readonly Dictionary<OperationType, HttpMethod> HttpMethodMap =
            new Dictionary<OperationType, HttpMethod>
            {
                [ OperationType.Post ] = HttpMethod.Post,
                [ OperationType.Get ] = HttpMethod.Get
            };

        private static readonly MediaTypeFormatterCollection SupportedMediaTypeFormatters =
            new MediaTypeFormatterCollection
            {
                new JsonMediaTypeFormatter(),
                new XmlMediaTypeFormatter(),
                new FormUrlEncodedMediaTypeFormatter()
            };

    }

    internal class OpenApiParameterComparer : IEqualityComparer<OpenApiParameter>
    {
        public bool Equals(OpenApiParameter x, OpenApiParameter y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(OpenApiParameter obj)
        {
            return obj.GetHashCode();
        }
    }
}