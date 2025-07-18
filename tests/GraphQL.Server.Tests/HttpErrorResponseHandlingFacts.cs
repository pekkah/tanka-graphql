using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class HttpErrorResponseHandlingFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory;
    private readonly HttpClient _client;

    public HttpErrorResponseHandlingFacts()
    {
        _factory = new TankaGraphQLServerFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostRequest_WithNullBody_Returns400BadRequest()
    {
        // Given
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(problemDetails);
        Assert.Contains("Could not parse GraphQL request", problemDetails.Detail);
    }

    [Fact]
    public async Task PostRequest_WithEmptyBody_Returns400BadRequest()
    {
        // Given
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(problemDetails);
        Assert.Contains("Could not parse GraphQL request", problemDetails.Detail);
    }

    [Fact]
    public async Task PostRequest_WithInvalidJson_Returns400BadRequest()
    {
        // Given
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(problemDetails);
        Assert.Contains("Could not parse GraphQL request", problemDetails.Title);
    }

    [Fact]
    public async Task PostRequest_WithMalformedJson_Returns400BadRequest()
    {
        // Given
        var content = new StringContent("{\"query\": \"{ hello", Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(problemDetails);
        Assert.Contains("Could not parse GraphQL request", problemDetails.Title);
    }

    [Fact]
    public async Task PostRequest_WithValidRequestButInvalidQuery_Returns200WithErrors()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ invalidField }"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Errors);
        Assert.NotEmpty(executionResult.Errors);
    }

    [Fact]
    public async Task PostRequest_WithValidRequest_Returns200WithData()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Data);
    }

    [Fact]
    public async Task PostRequest_WithNonJsonContentType_IsIgnored()
    {
        // Given
        var content = new StringContent("{ hello }", Encoding.UTF8, "text/plain");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        // The request should be ignored (not processed by GraphQL middleware)
        // The actual response will depend on your server configuration
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRequest_WithJsonContentType_IsProcessed()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/graphql")
        {
            Content = content
        });

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Response_IncludesElapsedTimeHeader()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Elapsed", response.Headers.GetValues("Elapsed").First());
        
        var elapsedValue = response.Headers.GetValues("Elapsed").First();
        Assert.EndsWith("ms", elapsedValue);
    }

    [Fact]
    public async Task Response_WithExecutionError_Returns200WithErrorDetails()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ nonExistentField }"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Errors);
        Assert.NotEmpty(executionResult.Errors);
    }

    [Fact]
    public async Task PostRequest_WithLargePayload_IsHandledCorrectly()
    {
        // Given
        var largeVariables = new Dictionary<string, object>();
        for (int i = 0; i < 1000; i++)
        {
            largeVariables[$"var{i}"] = $"value{i}";
        }
        
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }",
            Variables = largeVariables
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Data);
    }

    [Fact]
    public async Task PostRequest_WithComplexVariables_IsHandledCorrectly()
    {
        // Given
        var complexVariables = new Dictionary<string, object>
        {
            ["stringVar"] = "test",
            ["intVar"] = 42,
            ["boolVar"] = true,
            ["arrayVar"] = new[] { 1, 2, 3 },
            ["objectVar"] = new { nested = "value" }
        };
        
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }",
            Variables = complexVariables
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Data);
    }

    [Fact]
    public async Task PostRequest_WithOperationName_IsHandledCorrectly()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "query GetHello { hello }",
            OperationName = "GetHello"
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Data);
    }

    [Fact]
    public async Task PostRequest_WithUnsupportedMediaType_IsIgnored()
    {
        // Given
        var content = new StringContent("{ hello }", Encoding.UTF8, "application/xml");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        // The request should be ignored (not processed by GraphQL middleware)
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostRequest_WithSpecialCharacters_IsHandledCorrectly()
    {
        // Given
        var graphqlRequest = new GraphQLHttpRequest
        {
            Query = "{ hello }",
            Variables = new Dictionary<string, object>
            {
                ["specialChars"] = "Test with special chars: áéíóú ñ ¡¿ 中文 русский"
            }
        };
        
        var json = JsonSerializer.Serialize(graphqlRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // When
        var response = await _client.PostAsync("/graphql", content);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var executionResult = JsonSerializer.Deserialize<ExecutionResult>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        Assert.NotNull(executionResult);
        Assert.NotNull(executionResult.Data);
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
    }
}