using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Extensions.Tracing;
using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Server.Tests.Usages;

public class ServerBuilderUsageFacts
{
    public ServerBuilderUsageFacts()
    {
        Services = new ServiceCollection();
        Services.AddLogging();
        Services.AddMemoryCache();
    }

    public IServiceCollection Services { get; set; }

    [Fact]
    public void AddTankaGraphQL()
    {
        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => default);

        /* Then */
        var provider = Services.BuildServiceProvider();

        // Options is added by the AddTankaGraphQL
        Assert.NotNull(provider.GetService<IOptions<ServerOptions>>());

        // Query stream service is added by AddTankaGraphQL
        Assert.NotNull(provider.GetService<IQueryStreamService>());
    }

    [Fact]
    public async Task Configure_Schema()
    {
        /* Given */
        var schema = Substitute.For<ISchema>();

        /* When */
        Services.AddTankaGraphQL()
            // simple factory function for setting up the schema
            .ConfigureSchema(() => new ValueTask<ISchema>(schema));

        /* Then */
        var provider = Services.BuildServiceProvider();
        var options = provider.GetService<IOptions<ServerOptions>>().Value;
        var actual = await options.GetSchema(null);
        Assert.Same(schema, actual);
    }

    [Fact]
    public async Task Configure_Schema_with_dependency()
    {
        /* Given */
        var schema = Substitute.For<ISchema>();

        /* When */
        Services.AddTankaGraphQL()
            // factory function with one dependency resolved from service provider
            .ConfigureSchema<IMemoryCache>(async cache =>
                await cache.GetOrCreateAsync("schema", entry => Task.FromResult(schema)));

        /* Then */
        var provider = Services.BuildServiceProvider();
        var options = provider.GetService<IOptions<ServerOptions>>().Value;
        var actual = await options.GetSchema(null);
        Assert.Same(schema, actual);
    }

    [Fact]
    public void Configure_Rules()
    {
        /* Given */
        var schema = Substitute.For<ISchema>();
        var maxCost = CostAnalyzer.MaxCost(100);

        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => new ValueTask<ISchema>(schema))
            // rules factory function with the default rules as the parameter
            .ConfigureRules(rules => rules.Concat(new[]
            {
                // append max query cost validation rule
                maxCost
            }).ToArray());

        /* Then */
        var provider = Services.BuildServiceProvider();
        var options = provider.GetService<IOptions<ServerOptions>>().Value;
        var actual = options.ValidationRules;
        Assert.Contains(actual, rule => rule == maxCost);
    }

    [Fact]
    public void Configure_Rules_remove_all()
    {
        /* Given */
        var schema = Substitute.For<ISchema>();

        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => new ValueTask<ISchema>(schema))
            // rules factory function with the default rules as the parameter
            .ConfigureRules(rules => new CombineRule[0]);

        /* Then */
        var provider = Services.BuildServiceProvider();
        var options = provider.GetService<IOptions<ServerOptions>>().Value;
        var actual = options.ValidationRules;
        Assert.Empty(actual);
    }


    [Fact]
    public void Configure_Rules_with_dependency()
    {
        /* Given */
        var schema = Substitute.For<ISchema>();

        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => new ValueTask<ISchema>(schema))
            // rules factory function with default rules and dependency resolved from service provider
            .ConfigureRules<ILogger<ServerBuilderUsageFacts>>((rules, logger) => new CombineRule[0]);

        /* Then */
        var provider = Services.BuildServiceProvider();
        var options = provider.GetService<IOptions<ServerOptions>>().Value;
        var actual = options.ValidationRules;
        Assert.Empty(actual);
    }

    [Fact]
    public void Add_Extension()
    {
        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => default)
            // add trace execution extension
            .AddExtension<TraceExtension>();

        /* Then */
        var provider = Services.BuildServiceProvider();
        var executorExtensions = provider.GetService<IEnumerable<IExecutorExtension>>();
        Assert.Contains(executorExtensions, extension => extension is TraceExtension);
    }

    [Fact]
    public void Configure_WebSockets()
    {
        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => default)
            // Add websocket services with defaults
            .ConfigureWebSockets();

        /* Then */
        var provider = Services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<WebSocketServer>());
        Assert.NotNull(provider.GetService<IOptions<WebSocketServerOptions>>());
    }

    [Fact]
    public async Task Configure_WebSockets_with_Accept()
    {
        /* Given */
        var called = false;
        /* When */
        Services.AddTankaGraphQL()
            .ConfigureSchema(() => default)
            // Add websockets services with accept method
            .ConfigureWebSockets(async context =>
            {
                called = true;
                var succeeded = true; //todo: authorize

                if (succeeded)
                {
                    await context.Output.WriteAsync(new OperationMessage
                    {
                        Type = MessageType.GQL_CONNECTION_ACK
                    });
                }
                else
                {
                    // you must decide what kind of message to send back to the client
                    // in case the connection is not accepted.
                    await context.Output.WriteAsync(new OperationMessage
                    {
                        Type = MessageType.GQL_CONNECTION_ERROR,
                        Id = context.Message.Id
                    });

                    // complete the output forcing the server to disconnect
                    context.Output.Complete();
                }
            });

        /* Then */
        var provider = Services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<WebSocketServer>());
        var options = provider.GetRequiredService<IOptions<WebSocketServerOptions>>().Value;
        await options.AcceptAsync(new MessageContext(new OperationMessage(),
            Channel.CreateUnbounded<OperationMessage>()));
        Assert.True(called);
    }

    [Fact]
    public void Validate_Schema_provided()
    {
        /* When */
        Services.AddTankaGraphQL();

        /* Then */
        var provider = Services.BuildServiceProvider();
        var exception =
            Assert.Throws<OptionsValidationException>(() => provider.GetService<IOptions<ServerOptions>>().Value);

        Assert.Contains(exception.Failures, failure => failure.Contains(nameof(ServerOptions.GetSchema)));
    }
}