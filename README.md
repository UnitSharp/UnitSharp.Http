# Package

https://www.nuget.org/packages/UnitSharp.Http/

# Stub

```cs
var content = new
{
    Id = Guid.NewGuid(),
    Name = "Obiwan Kenobi",
    Nickname = "Ben",
};
string hostAddress = "https://api.test.com";
string localPath = $"api/items/{content.Id}";
```

## GET with JSON response content

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath).RespondsJson(content);

// Act
var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync(localPath);
```

## GET with query string

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
var query = new { bar = "baz" };
stub.Get(hostAddress, localPath, query).RespondsJson(content);

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
    .RespondsJson(content);

// Act
var authorization = new AuthenticationHeaderValue(scheme, parameter);
var client = new HttpClient(handler: stub)
{
    BaseAddress = new Uri(hostAddress),
    DefaultRequestHeaders = { Authorization = authorization },
};
HttpResponseMessage response = await client.GetAsync(localPath);
```

## GET with status code

```cs
// Arrange
var stub = new HttpMessageHandlerStub();
stub.Get(hostAddress, localPath).Responds(HttpStatusCode.NotFound);

// Act
var client = new HttpClient(handler: stub) { BaseAddress = new Uri(hostAddress) };
HttpResponseMessage response = await client.GetAsync(localPath);
```
