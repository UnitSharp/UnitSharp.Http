using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UnitSharp.Http
{
    internal sealed class HttpRequestExcerpt
    {
        private HttpRequestExcerpt(
            HttpMethod method,
            Uri requestUri,
            HttpRequestHeaders headers)
        {
            Method = method;
            RequestUri = requestUri;
            Headers = headers;
        }

        public HttpMethod Method { get; }

        public Uri RequestUri { get; }

        public HttpRequestHeaders Headers { get; }

        public static HttpRequestExcerpt Create(HttpRequestMessage request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new HttpRequestExcerpt(
                request.Method,
                request.RequestUri,
                request.Headers);
        }
    }
}
