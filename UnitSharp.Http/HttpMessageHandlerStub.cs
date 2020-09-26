using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitSharp.Http
{
    public sealed class HttpMessageHandlerStub : HttpMessageHandler
    {
        private readonly List<HttpRequestHandler> handlers;

        public HttpMessageHandlerStub()
            => handlers = new List<HttpRequestHandler>();

        internal void Configure(HttpRequestHandler handler)
            => handlers.Insert(index: 0, handler);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var excerpt = HttpRequestExcerpt.Create(request);

            foreach (HttpRequestHandler handler in handlers)
            {
                if (handler.CanHandle(excerpt))
                {
                    return await handler.Handle(request);
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
