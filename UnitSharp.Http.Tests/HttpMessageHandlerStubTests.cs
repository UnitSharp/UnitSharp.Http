using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    }
}
