using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UnitSharp.Http
{
    public static class HttpMessageHandlerDsl
    {
        public static GetClause Get(
            this HttpMessageHandlerStub stub,
            Uri hostAddress,
            string localPath,
            IReadOnlyDictionary<string, StringValues> query)
        {
            return new GetClause(stub, Match);

            bool Match(HttpRequestExcerpt excerpt)
                => excerpt.RequestUri.Scheme == hostAddress.Scheme
                && excerpt.RequestUri.Host == hostAddress.Host
                && MatchLocalPath(excerpt.RequestUri, localPath)
                && MatchQuery(excerpt.RequestUri, query);
        }

        private static bool MatchLocalPath(Uri requestUri, string localPath)
        {
            return requestUri.LocalPath == localPath
                || requestUri.LocalPath.TrimStart('/') == localPath;
        }

        private static bool MatchQuery(
            Uri requestUri, IReadOnlyDictionary<string, StringValues> query)
        {
            NameValueCollection requestQuery = HttpUtility.ParseQueryString(requestUri.Query);

            if (query.Count != (requestQuery.HasKeys() ? requestQuery.Keys.Count : 0))
            {
                return false;
            }

            foreach ((string name, StringValues values) in query)
            {
                if (requestQuery.HasKeys() == false ||
                    requestQuery.Keys.Cast<string>().Contains(name) == false ||
                    (StringValues)requestQuery.GetValues(name) != values)
                {
                    return false;
                }
            }

            return true;
        }

        public static GetClause Get(
            this HttpMessageHandlerStub stub,
            Uri hostAddress)
        {
            return stub.Get(
                hostAddress,
                localPath: "",
                query: new Dictionary<string, StringValues>());
        }

        public static GetClause Get(
            this HttpMessageHandlerStub stub,
            Uri hostAddress,
            string localPath)
        {
            return stub.Get(
                hostAddress,
                localPath,
                query: new Dictionary<string, StringValues>());
        }

        public static GetClause Get(
            this HttpMessageHandlerStub stub,
            Uri hostAddress,
            string localPath,
            IReadOnlyDictionary<string, object> query)
        {
            return stub.Get(
                hostAddress,
                localPath,
                query.ToDictionary(t => t.Key, t => GetQueryValue(t.Value)));
        }

        public static GetClause Get(
            this HttpMessageHandlerStub stub,
            Uri hostAddress,
            string localPath,
            object query)
        {
            PropertyInfo[] properties = query
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return stub.Get(
                hostAddress,
                localPath,
                query: properties.ToDictionary(p => p.Name, p => p.GetValue(query)));
        }

        private static StringValues GetQueryValue(object value)
        {
            if (value is object[] values)
            {
                return new StringValues(values.Select(x => $"{x}").ToArray());
            }

            return new StringValues($"{value}");
        }

        public static GetClause WithAuthorization(
            this GetClause dsl,
            Func<AuthenticationHeaderValue, bool> predicate)
        {
            return new GetClause(dsl.Stub, CanHandle);

            bool CanHandle(HttpRequestExcerpt request)
                => dsl.CanHandle.Invoke(request)
                && predicate.Invoke(request.Headers.Authorization);
        }

        public static GetClause WithAuthorization(
            this GetClause dsl,
            string scheme,
            string parameter)
        {
            return new GetClause(dsl.Stub, CanHandle);

            bool CanHandle(HttpRequestExcerpt request)
                => dsl.CanHandle.Invoke(request)
                && request.Headers.Authorization != null
                && request.Headers.Authorization.Scheme == scheme
                && request.Headers.Authorization.Parameter == parameter;
        }

        public static HttpMessageHandlerStub Responds(
            this GetClause dsl,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> handle)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, handle));
            return stub;
        }

        public static HttpMessageHandlerStub Responds(
            this GetClause dsl,
            Func<HttpRequestMessage, HttpResponseMessage> handle)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, handle));
            return stub;
        }

        public static HttpMessageHandlerStub Responds(
            this GetClause dsl,
            HttpResponseMessage response)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, response));
            return stub;
        }

        public static HttpMessageHandlerStub Responds(
            this GetClause dsl,
            HttpStatusCode statusCode)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            var response = new HttpResponseMessage(statusCode);
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, response));
            return stub;
        }

        public static HttpMessageHandlerStub RespondsJson(
            this GetClause dsl,
            object value)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            HttpResponseMessage response = CreateJsonResponse(value);
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, response));
            return stub;
        }

        private static HttpResponseMessage CreateJsonResponse(object value)
        {
            HttpContent content = CreateJsonContent(value);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }

        private static HttpContent CreateJsonContent(object value)
        {
            string json = JsonConvert.SerializeObject(value);
            return new StringContent(json, Encoding.UTF8)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                    {
                        CharSet = "utf-8",
                    },
                },
            };
        }

        public static HttpMessageHandlerStub RespondsStream(
            this GetClause dsl,
            MediaTypeHeaderValue contentType,
            Stream content)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            HttpResponseMessage response = CreateStreamResponse(contentType, content);
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, response));
            return stub;
        }

        private static HttpResponseMessage CreateStreamResponse(
            MediaTypeHeaderValue contentType,
            Stream content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = CreateStreamContent(contentType, content),
            };
        }

        private static StreamContent CreateStreamContent(
            MediaTypeHeaderValue contentType,
            Stream content)
        {
            return new StreamContent(content)
            {
                Headers =
                {
                    ContentType = contentType,
                },
            };
        }

        public static HttpMessageHandlerStub RespondsJsonStream(
            this GetClause dsl,
            Stream content)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            var contentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
            HttpResponseMessage response = CreateStreamResponse(contentType, content);
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, response));
            return stub;
        }
    }
}
