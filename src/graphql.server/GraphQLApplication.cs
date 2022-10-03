﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tanka.GraphQL.Language;

namespace Tanka.GraphQL.Server;

public class GraphQLApplication
{
    private readonly IOptionsMonitor<GraphQLApplicationOptions> _optionsMonitor;
    private readonly IEnumerable<IGraphQLTransport> _transports;

    public GraphQLApplication(
        IOptionsMonitor<GraphQLApplicationOptions> optionsMonitor,
        SchemaCollection schemas,
        IEnumerable<IGraphQLTransport> transports)
    {
        _optionsMonitor = optionsMonitor;
        _transports = transports;
        Schemas = schemas;
    }

    public SchemaCollection Schemas { get; }

    public RouteHandlerBuilder Map(
        string pattern,
        IEndpointRouteBuilder routes,
        GraphQLRequestPipelineBuilder builder)
    {
        var builders = new List<IEndpointConventionBuilder>();
        foreach (var transport in _transports)
            builders.Add(transport.Map(pattern, routes, builder));

        if (_optionsMonitor.CurrentValue.EnableUi)
            builders.Add(routes.MapGet($"{pattern}/ui", async context =>
            {
                var htmlStream = typeof(GraphQLApplication)
                    .Assembly.GetManifestResourceStream("Tanka.GraphQL.Server.GraphiQL.host.html");

                await htmlStream.CopyToAsync(context.Response.Body);
            }));

        return new RouteHandlerBuilder(builders);
    }

    public IEndpointConventionBuilder Map(string pattern, string schemaName, IEndpointRouteBuilder routes)
    {
        return Map(pattern, routes, CreateDefaultPipelineBuilder(schemaName, routes.ServiceProvider));
    }

    private GraphQLRequestPipelineBuilder CreateDefaultPipelineBuilder(
        string schemaName,
        IServiceProvider services)
    {
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.Use(next => context =>
        {
            return UseRequestServices();

            async IAsyncEnumerable<ExecutionResult> UseRequestServices()
            {
                await using var scope = builder.ApplicationServices.CreateAsyncScope();
                context.RequestServices = scope.ServiceProvider;

                await foreach (var item in next(context))
                    yield return item;
            }
        });

        builder.Use(next => context =>
        {
            context.Schema = Schemas.Get(schemaName);
            context.Document = context.Query;
            context.Operation = Operations.GetOperation(context.Document, context.OperationName);

            return next(context);
        });


        builder.Use(next => context =>
        {
            return Execute();

            async IAsyncEnumerable<ExecutionResult> Execute()
            {
                var result = await Executor.ExecuteAsync(new ExecutionOptions
                {
                    OperationName = context.OperationName,
                    Document = context.Query,
                    VariableValues = context.Variables,
                    Schema = context.Schema
                }, CancellationToken.None);

                yield return result;
            }
        });

        return builder;
    }
}