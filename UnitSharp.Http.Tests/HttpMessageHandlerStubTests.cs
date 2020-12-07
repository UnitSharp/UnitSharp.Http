using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static System.Web.HttpUtility;

namespace UnitSharp.Http
{
    [TestClass]
    public class HttpMessageHandlerStubTests
    {
        [TestMethod, AutoData]
        public async Task Get_accepts_consistent_host_address(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            HttpResponseMessage actual = await client.GetAsync(requestUri: "");

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_does_not_accept_inconsistent_host_address(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://unit.teset.com"),
            };

            // Act
            HttpResponseMessage actual = await client.GetAsync(requestUri: "");

            // Assert
            actual.Should().NotBeSameAs(response);
        }

        [TestMethod]
        [InlineAutoData("local-path", "local-path")]
        [InlineAutoData("local-path", "/local-path")]
        [InlineAutoData("/local-path", "local-path")]
        [InlineAutoData("/local-path", "/local-path")]
        public async Task Get_accepts_consistent_local_path(
            string localPath,
            string validPath,
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            HttpResponseMessage actual = await client.GetAsync(localPath);

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_does_not_accept_inconsistent_local_path(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            string wrongPath)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            HttpResponseMessage actual = await client.GetAsync(wrongPath);

            // Assert
            actual.Should().NotBeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_accepts_consistent_query(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            IReadOnlyDictionary<string, StringValues> query)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath, query).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            IEnumerable<string> queryElements =
                from t in query.Shuffle()
                select $"{UrlEncode(t.Key)}={UrlEncode(t.Value)}";
            string requestUri = $"{localPath}?{string.Join('&', queryElements)}";
            HttpResponseMessage actual = await client.GetAsync(requestUri);

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_does_not_accept_inconsistent_query(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            string inconsistentValue,
            IReadOnlyDictionary<string, StringValues> query)
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath, query).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            IEnumerable<string> queryElements =
                from t in query.Select((t, i) => (t.Key, Value: CrackValue(i, t.Value))).Shuffle()
                select $"{UrlEncode(t.Key)}={UrlEncode(t.Value)}";

            StringValues CrackValue(int index, StringValues originalValue)
                => index == 0 ? new StringValues(inconsistentValue) : originalValue;

            string requestUri = $"{localPath}?{string.Join('&', queryElements)}";
            HttpResponseMessage actual = await client.GetAsync(requestUri);

            // Assert
            actual.Should().NotBeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_accepts_object_query_values(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            int value1,
            string value2)
        {
            // Arrange
            var query = new Dictionary<string, object>
            {
                [nameof(value1)] = value1,
                [nameof(value2)] = value2,
            };
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath, query).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            string requestUri = $"{localPath}?{nameof(value1)}={value1}&{nameof(value2)}={value2}";
            HttpResponseMessage actual = await client.GetAsync(requestUri);

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_accepts_object_array_query_value(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            int value1,
            string value2)
        {
            // Arrange
            var query = new Dictionary<string, object>
            {
                ["value"] = new object[] { value1, value2 },
            };
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath, query).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            string requestUri = $"{localPath}?value={value1}&value={value2}";
            HttpResponseMessage actual = await client.GetAsync(requestUri);

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Get_accepts_anonymous_object_query(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            int value1,
            string value2)
        {
            // Arrange
            var query = new { value1, value2 };
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            handler.Get(hostAddress, localPath, query).Responds(_ => Task.FromResult(response));
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            // Act
            string requestUri = $"{localPath}?{nameof(value1)}={value1}&{nameof(value2)}={value2}";
            HttpResponseMessage actual = await client.GetAsync(requestUri);

            // Assert
            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Responds_with_get_clause_accepts_http_response_message_function(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            var response = new HttpResponseMessage();
            handler.Get(hostAddress).Responds(_ => response);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task Responds_with_get_clause_accepts_http_response_message(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            var response = new HttpResponseMessage();
            handler.Get(hostAddress).Responds(response);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_predicate_excludes_invalid_authorization_header(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Get(hostAddress).WithAuthorization(_ => false).Responds(response);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_predicate_includes_valid_authorization_header(
            HttpMessageHandlerStub handler,
            Uri hostAddress)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Get(hostAddress).WithAuthorization(_ => true).Responds(response);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_predicate_keeps_original_predicate(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Get(hostAddress).WithAuthorization(_ => true).Responds(response);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(localPath);

            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_value_excludes_invalid_authorization_header(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            AuthenticationHeaderValue authorization,
            Generator<string> generator)
        {
            // Arrange
            string scheme = generator.First(x => x != authorization.Scheme);
            string parameter = generator.First(x => x != authorization.Parameter);

            handler.Get(hostAddress)
                   .WithAuthorization(scheme, parameter)
                   .Responds(new HttpResponseMessage(HttpStatusCode.OK));

            var client = new HttpClient(handler)
            {
                BaseAddress = hostAddress,
                DefaultRequestHeaders = { Authorization = authorization },
            };

            // Act
            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_value_includes_valid_authorization_header(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string scheme,
            string parameter)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Get(hostAddress).WithAuthorization(scheme, parameter).Responds(response);
            var authorization = new AuthenticationHeaderValue(scheme, parameter);
            var client = new HttpClient(handler)
            {
                BaseAddress = hostAddress,
                DefaultRequestHeaders = { Authorization = authorization },
            };

            HttpResponseMessage actual = await client.GetAsync(string.Empty);

            actual.Should().BeSameAs(response);
        }

        [TestMethod, AutoData]
        public async Task WithAuthorization_with_value_keeps_original_predicate(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string scheme,
            string parameter,
            string localPath)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Get(hostAddress).WithAuthorization(scheme, parameter).Responds(response);
            var authorization = new AuthenticationHeaderValue(scheme, parameter);
            var client = new HttpClient(handler)
            {
                BaseAddress = hostAddress,
                DefaultRequestHeaders = { Authorization = authorization },
            };

            HttpResponseMessage actual = await client.GetAsync(localPath);

            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod, AutoData]
        public async Task RespondsJson_with_get_clause_correctly_configure_response(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            Guid[] value)
        {
            handler.Get(hostAddress, localPath).RespondsJson(value);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(localPath);

            actual.StatusCode.Should().Be(HttpStatusCode.OK);
            actual.Content.Should().NotBeNull();
            actual.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            actual.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
            Guid[] content = await actual.Content.ReadAsAsync<Guid[]>();
            content.Should().BeEquivalentTo(value);
        }

        [TestMethod, AutoData]
        public async Task Responds_with_status_code_correctly_configures_response(
            HttpMessageHandlerStub handler,
            Uri hostAddress,
            string localPath,
            HttpStatusCode statusCode)
        {
            handler.Get(hostAddress, localPath).Responds(statusCode);
            var client = new HttpClient(handler) { BaseAddress = hostAddress };

            HttpResponseMessage actual = await client.GetAsync(localPath);

            actual.StatusCode.Should().Be(statusCode);
            actual.Content.Should().BeNull();
        }
    }
}
