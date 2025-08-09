using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLHttpTransportSecurityTests : IClassFixture<TankaGraphQLServerFactory>
{
    private readonly TankaGraphQLServerFactory _factory;

    public GraphQLHttpTransportSecurityTests(TankaGraphQLServerFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Not_Leak_Exception_Details_In_Error_Response()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create malformed JSON that will trigger a parsing exception
        var malformedJson = "{ invalid json that will cause detailed parsing error }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/graphql", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Verify that the response contains only generic error message
        Assert.NotNull(problemDetails);
        Assert.Equal("Could not parse GraphQL request from body of the request", problemDetails.Title);
        Assert.Equal("Invalid request format or content", problemDetails.Detail);

        // Verify that sensitive implementation details are NOT leaked
        // Should not contain detailed JSON parsing error messages
        Assert.DoesNotContain("LineNumber", responseContent);
        Assert.DoesNotContain("BytePositionInLine", responseContent);
        Assert.DoesNotContain("JsonException", responseContent);
        Assert.DoesNotContain("JsonReaderException", responseContent);
        Assert.DoesNotContain("Exception", responseContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Stack", responseContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.", responseContent);
        Assert.DoesNotContain("file://", responseContent, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at System", responseContent);
        Assert.DoesNotContain("ThrowHelper", responseContent);
    }

    [Fact]
    public async Task Should_Handle_Valid_Request_Normally_After_Security_Fix()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create a valid GraphQL request
        var validRequest = new
        {
            query = "{ __typename }"
        };
        var json = JsonSerializer.Serialize(validRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/graphql", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("__typename", responseContent);
    }
}