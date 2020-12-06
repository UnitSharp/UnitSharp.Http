```cs
string hostAddress = "https://api.test.com";
string localPath = "api/foo";

var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath, query: new { bar = "baz" } )
    .Responds(new HttpResponseMessage(HttpStatusCode.NoContent));

var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync($"{localPath}?bar=baz");
```
