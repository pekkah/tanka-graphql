using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

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


    public IEndpointConventionBuilder MapDefault(string pattern, string schemaName, IEndpointRouteBuilder routes)
    {
        var builders = new List<IEndpointConventionBuilder>();
        foreach (IGraphQLTransport transport in _transports)
            builders.Add(MapTransport(pattern, routes, transport, b => ConfigureDefaultPipeline(b, schemaName)));

        if (_optionsMonitor.CurrentValue.EnableUi)
            builders.Add(routes.MapGet($"{pattern}/ui", CreateUiDelegate(pattern)));

        return new RouteHandlerBuilder(builders);
    }

    public IEndpointConventionBuilder Map(string pattern, IEndpointRouteBuilder routes, Action<GraphQLRequestPipelineBuilder> configureRequest)
    {
        var builders = new List<IEndpointConventionBuilder>();
        foreach (IGraphQLTransport transport in _transports)
            builders.Add(MapTransport(pattern, routes, transport, configureRequest));

        if (_optionsMonitor.CurrentValue.EnableUi)
            builders.Add(routes.MapGet($"{pattern}/ui", CreateUiDelegate(pattern)));

        return new RouteHandlerBuilder(builders);
    }


    private void ConfigureDefaultPipeline(GraphQLRequestPipelineBuilder builder, string schemaName)
    {
        builder.UseDefaults(schemaName);
    }

    private RequestDelegate CreateUiDelegate(string apiPattern)
    {
        //todo: needs cleanup
        Stream? htmlStream = typeof(GraphQLApplication)
            .Assembly.GetManifestResourceStream("Tanka.GraphQL.Server.GraphiQL.host.html");

        var reader = new StreamReader(htmlStream);
        string htmlTemplate = reader.ReadToEnd();


        return async context =>
        {
            string requestUrl = context.Request.GetEncodedUrl();
            string html = htmlTemplate.Replace("{{httpUrl}}", requestUrl.Replace("/ui", string.Empty));

            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        };
    }

    private IEndpointConventionBuilder MapTransport(
        string pattern,
        IEndpointRouteBuilder routes,
        IGraphQLTransport transport,
        Action<GraphQLRequestPipelineBuilder> configureRequest)
    {
        var pipelineBuilder = new GraphQLRequestPipelineBuilder(routes.ServiceProvider);
        transport.Build(pipelineBuilder);
        configureRequest(pipelineBuilder);

        return transport.Map(pattern, routes, pipelineBuilder.Build());
    }
}