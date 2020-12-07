# Stub

```cs
string hostAddress = "https://api.test.com";
string localPath = "api/foo";
```

## GET

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath)
    .Responds(new HttpResponseMessage(HttpStatusCode.NoContent));

// Act
var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync(localPath);
```

## GET with query string

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath, query: new { bar = "baz" })
    .Responds(new HttpResponseMessage(HttpStatusCode.NoContent));

// Act
var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync($"{localPath}?bar=baz");
```

## GET with authorization header

```cs
// Arrange
var scheme = "bearar";
var parameter = "<your token>";

var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath, query)
    .WithAuthorization(scheme, parameter)
    .Responds(new HttpResponseMessage(HttpStatusCode.NoContent));

// Act
var authorization = new AuthenticationHeaderValue(scheme, parameter);
var client = new HttpClient(handler: stub)
{
    BaseAddress = new Uri(hostAddress),
    DefaultRequestHeaders = { Authorization = authorization },
};
HttpResponseMessage response = await client.GetAsync(localPath);
```

## GET with JSON response content

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath).RespondsJson(new
{
    Id = Guid.NewGuid(),
    Name = "Obiwan Kenobi",
    Nickname = "Ben",
});

// Act
var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync(localPath);
```
