using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class SchemaInitializerFacts
{
    [Fact]
    public async Task First_request_after_startup_should_have_schema_available()
    {
        /* Given: a schema which takes a while to initialize */
        await using var factory = new TankaGraphQLServerFactory()
            .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                services.Configure<SchemaOptions>("Default", _ => Thread.Sleep(500))));

        using var client = factory.CreateClient();

        /* When: the first request is sent right after startup */
        using var response = await client.PostAsync(
            "/graphql",
            new StringContent("""{"query":"{ hello }"}""", Encoding.UTF8, "application/json"));

        /* Then */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
