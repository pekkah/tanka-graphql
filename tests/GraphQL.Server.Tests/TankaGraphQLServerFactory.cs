using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server.Tests;

public class TankaGraphQLServerFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add services
        });

        builder.UseContentRoot(Directory.GetCurrentDirectory());
    }

    public EventAggregator<IEvent> Events => Services.GetRequiredService<EventAggregator<IEvent>>();

    public WebSocketClient CreateWebSocketClient() => Server.CreateWebSocketClient();
}