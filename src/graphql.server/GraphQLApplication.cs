using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL.Server;

public class GraphQLApplication
{
    private readonly IOptionsMonitor<GraphQLApplicationOptions> _optionsMonitor;
    private readonly IEnumerable<IGraphQLTransport> _transports;

    public GraphQLApplication(
        IOptionsMonitor<GraphQLApplicationOptions> optionsMonitor,
        IEnumerable<IGraphQLTransport> transports)
    {
        _optionsMonitor = optionsMonitor;
        _transports = transports;
    }

    public RouteHandlerBuilder Map(
        string pattern,
        IEndpointRouteBuilder routes,
        GraphQLRequestPipelineBuilder builder)
    {
        var builders = new List<IEndpointConventionBuilder>();
        foreach (var transport in _transports)
            builders.Add(transport.Map(pattern, routes, builder));

        if (_optionsMonitor.CurrentValue.EnableUi)
            builders.Add(routes.MapGet($"{pattern}/ui", CreateUiDelegate(pattern)));

        return new(builders);
    }

    private RequestDelegate CreateUiDelegate(string apiPattern)
    {
        var htmlStream = typeof(GraphQLApplication)
            .Assembly.GetManifestResourceStream("Tanka.GraphQL.Server.GraphiQL.host.html");

        var reader = new StreamReader(htmlStream);
        var htmlTemplate = reader.ReadToEnd();
        var html = htmlTemplate.Replace("{{httpUrl}}", apiPattern);

        return async context =>
        {
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        };
    }

    public IEndpointConventionBuilder Map(string pattern, string schemaName, IEndpointRouteBuilder routes)
    {
        return Map(pattern, routes, CreateDefaultPipelineBuilder(schemaName, routes.ServiceProvider));
    }

    public IEndpointConventionBuilder Map(string pattern, IEndpointRouteBuilder routes, Action<GraphQLRequestPipelineBuilder> configureRequest)
    {
        var builder = new GraphQLRequestPipelineBuilder(routes.ServiceProvider);
        configureRequest(builder);
        builder.RunExecutor();

        return Map(pattern, routes, builder);
    }

    private GraphQLRequestPipelineBuilder CreateDefaultPipelineBuilder(
        string schemaName,
        IServiceProvider services)
    {
        
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        builder.UseSchema(schemaName);
        builder.UseDefaultOperationResolver();
        builder.UseDefaultVariableCoercer();
        builder.UseSelectionSetPipeline(sets =>
        {
            sets.UseSelectionSetExecutor();
        });

        builder.RunExecutor();

        return builder;
    }
}