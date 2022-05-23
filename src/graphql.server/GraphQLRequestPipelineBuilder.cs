using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL.Server;

public class GraphQLRequestPipelineBuilder
{
    public IServiceProvider ApplicationServices { get; }

    private readonly List<Func<GraphQLRequestDelegate, GraphQLRequestDelegate>> _components = new();

    public GraphQLRequestPipelineBuilder(
        IServiceProvider applicationServices)
    {
        ApplicationServices = applicationServices;
    }

    public GraphQLRequestPipelineBuilder Use(Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    
    public GraphQLRequestDelegate Build()
    {
        GraphQLRequestDelegate pipeline = _ => throw new QueryExecutionException(
            "Request execution pipeline error. No middleware returned any results.",
            new NodePath());

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }
}