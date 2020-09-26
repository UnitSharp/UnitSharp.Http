using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

        public static HttpMessageHandlerStub Responds(
            this GetClause dsl,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> handle)
        {
            HttpMessageHandlerStub stub = dsl.Stub;
            stub.Configure(new HttpRequestHandler(dsl.CanHandle, handle));
            return stub;
        }
    }
}
