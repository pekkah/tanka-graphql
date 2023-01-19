using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.Server;

public class GraphQLRequestPipelineBuilder
{
    private readonly List<Func<GraphQLRequestDelegate, GraphQLRequestDelegate>> _components = new();

    public GraphQLRequestPipelineBuilder(
        IServiceProvider applicationServices)
    {
        ApplicationServices = applicationServices;
    }

    public IServiceProvider ApplicationServices { get; }

    public GraphQLRequestPipelineBuilder Use(Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }


    public GraphQLRequestDelegate Build()
    {
        GraphQLRequestDelegate pipeline = _ => throw new QueryException(
            "Request execution pipeline error. No middleware returned any results.")
        {
            Path = new()
        };

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }
}