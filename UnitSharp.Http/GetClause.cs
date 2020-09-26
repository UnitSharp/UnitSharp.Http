using System;

namespace UnitSharp.Http
{
    public sealed class GetClause
    {
        internal GetClause(
            HttpMessageHandlerStub stub,
            Func<HttpRequestExcerpt, bool> canHandle)
        {
            Stub = stub;
            CanHandle = canHandle;
        }

        internal HttpMessageHandlerStub Stub { get; }

        internal Func<HttpRequestExcerpt, bool> CanHandle { get; }
    }
}
