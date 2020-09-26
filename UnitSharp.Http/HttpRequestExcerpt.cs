using System;
using System.Net.Http;

namespace UnitSharp.Http
{
    internal sealed class HttpRequestExcerpt
    {
        private HttpRequestExcerpt(HttpMethod method, Uri requestUri)
        {
            Method = method;
            RequestUri = requestUri;
        }

        public HttpMethod Method { get; }

        public Uri RequestUri { get; }

        public static HttpRequestExcerpt Create(HttpRequestMessage request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new HttpRequestExcerpt(request.Method, request.RequestUri);
        }
    }
}
