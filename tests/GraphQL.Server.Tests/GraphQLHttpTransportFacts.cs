using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Tanka.GraphQL.Mock.Data;

using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLHttpTransportFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _testOutputHelper;

    public GraphQLHttpTransportFacts(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = new TankaGraphQLServerFactory();
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task POST_ValidQuery_ReturnsSuccessfulResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldMatchJson(@"
        {
          ""data"": {
            ""hello"": ""Hello World!""
          }
        }");
    }

    [Fact]
    public async Task GET_ValidQuery_ReturnsSuccessfulResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/graphql")
        {
            Content = content
        });

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldMatchJson(@"
        {
          ""data"": {
            ""hello"": ""Hello World!""
          }
        }");
    }

    [Fact]
    public async Task POST_QueryWithVariables_ReturnsSuccessfulResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query GetEvent($id: ID!) { event(id: $id) { id } }",
            Variables = new Dictionary<string, object> { { "id", "123" } }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldMatchJson(@"
        {
          ""data"": {
            ""event"": {
              ""id"": ""123""
            }
          }
        }");
    }

    [Fact]
    public async Task POST_EmptyBody_ReturnsBadRequest()
    {
        /* Given */
        var content = new StringContent("", Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.Equal("Could not parse GraphQL request from body of the request", 
            problemDetails.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task POST_InvalidJson_ReturnsBadRequest()
    {
        /* Given */
        var content = new StringContent("{ invalid json", Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.Equal("Could not parse GraphQL request from body of the request", 
            problemDetails.GetProperty("title").GetString());
    }

    [Fact]
    public async Task POST_NullRequest_ReturnsBadRequest()
    {
        /* Given */
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.Equal("Could not parse GraphQL request from body of the request", 
            problemDetails.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task POST_InvalidGraphQLQuery_ReturnsErrorResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { invalidField }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(result.TryGetProperty("errors", out var errors));
        Assert.True(errors.GetArrayLength() > 0);
    }

    [Fact]
    public async Task POST_QueryWithSyntaxError_ReturnsErrorResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello" // Missing closing brace
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(result.TryGetProperty("errors", out var errors));
        Assert.True(errors.GetArrayLength() > 0);
    }

    [Fact]
    public async Task POST_NonJsonContentType_IsIgnored()
    {
        /* Given */
        var content = new StringContent("some text", Encoding.UTF8, "text/plain");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentLength);
    }

    [Fact]
    public async Task POST_LargeQuery_ReturnsSuccessfulResponse()
    {
        /* Given */
        var largeQuery = "query { " + string.Join(" ", Enumerable.Repeat("hello", 1000)) + " }";
        var request = new GraphQLHttpRequest
        {
            Query = largeQuery
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        // Should have errors due to invalid query structure
        Assert.True(result.TryGetProperty("errors", out var errors));
        Assert.True(errors.GetArrayLength() > 0);
    }

    [Fact]
    public async Task POST_Mutation_ReturnsSuccessfulResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "mutation { addEvent(input: { type: INSERT, payload: \"test\" }) { id } }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        // Should have a successful mutation response
        Assert.True(result.TryGetProperty("data", out var data));
        Assert.True(data.TryGetProperty("addEvent", out var addEvent));
        Assert.True(addEvent.TryGetProperty("id", out var id));
        Assert.False(string.IsNullOrEmpty(id.GetString()));
    }

    [Fact]
    public async Task POST_ResponseHasElapsedHeader()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Elapsed"));
        
        var elapsedValue = response.Headers.GetValues("Elapsed").First();
        Assert.EndsWith("ms", elapsedValue);
        
        // Should be able to parse as a double
        var elapsedMs = double.Parse(elapsedValue.Replace("ms", ""));
        Assert.True(elapsedMs >= 0);
    }

    [Fact]
    public async Task POST_OperationName_ReturnsSuccessfulResponse()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query GetHello { hello } query GetOther { hello }",
            OperationName = "GetHello"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldMatchJson(@"
        {
          ""data"": {
            ""hello"": ""Hello World!""
          }
        }");
    }

    [Fact]
    public async Task POST_Subscription_ReturnsInternalServerError()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "subscription { events { id } }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.Equal("HttpTransport does not support multiple execution results", 
            problemDetails.GetProperty("title").GetString());
    }

    [Fact]
    public async Task POST_ContentTypeCheck_WorksCorrectly()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello }"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json; charset=utf-8");

        /* When */
        var response = await _httpClient.PostAsync("/graphql", content);

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldMatchJson(@"
        {
          ""data"": {
            ""hello"": ""Hello World!""
          }
        }");
    }

    [Fact]
    public async Task POST_ConcurrentRequests_AllSucceed()
    {
        /* Given */
        var request = new GraphQLHttpRequest
        {
            Query = "query { hello }"
        };

        var json = JsonSerializer.Serialize(request);
        var tasks = new List<Task<HttpResponseMessage>>();

        /* When */
        for (int i = 0; i < 10; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            tasks.Add(_httpClient.PostAsync("/graphql", content));
        }

        var responses = await Task.WhenAll(tasks);

        /* Then */
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.ShouldMatchJson(@"
            {
              ""data"": {
                ""hello"": ""Hello World!""
              }
            }");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
    }
}

public static class StringExtensions
{
    public static void ShouldMatchJson(this string actualJson, string expectedJson)
    {
        if (actualJson == null) throw new ArgumentNullException(nameof(actualJson));
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));

        var actualJsonObject = JObject.Parse(actualJson);
        var expectedJsonObject = JObject.Parse(expectedJson);

        var jsonEqual = JToken.DeepEquals(expectedJsonObject, actualJsonObject);
        Assert.True(jsonEqual, $"Expected: {expectedJsonObject}\r\nActual: {actualJsonObject}");
    }
}