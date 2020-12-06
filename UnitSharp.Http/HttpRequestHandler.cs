using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnitSharp.Http
{
    internal sealed class HttpRequestHandler
    {
        public HttpRequestHandler(
            Func<HttpRequestExcerpt, bool> canHandle,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> handle)
        {
            CanHandle = canHandle ?? throw new ArgumentNullException(nameof(canHandle));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));
        }

        public HttpRequestHandler(
            Func<HttpRequestExcerpt, bool> canHandle,
            Func<HttpRequestMessage, HttpResponseMessage> handle)
            : this(canHandle, request => Task.FromResult(handle.Invoke(request)))
        {
        }

        public HttpRequestHandler(
            Func<HttpRequestExcerpt, bool> canHandle,
            HttpResponseMessage response)
            : this(canHandle, _ => Task.FromResult(response))
        {
        }

        public Func<HttpRequestExcerpt, bool> CanHandle { get; }

        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Handle { get; }
    }
}
