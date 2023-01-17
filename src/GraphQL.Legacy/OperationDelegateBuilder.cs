using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL;

public class DiOperationDelegateBuilder: OperationDelegateBuilder
{
    public IServiceProvider ApplicationServices { get; }

    public DiOperationDelegateBuilder(
        IServiceProvider applicationServices)
    {
        ApplicationServices = applicationServices;
    }
}

public class OperationDelegateBuilder
{
    private readonly List<Func<OperationDelegate, OperationDelegate>> _components = new();

    public OperationDelegateBuilder Use(Func<OperationDelegate, OperationDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public OperationDelegate Build()
    {
        OperationDelegate pipeline = (_, _) => throw new QueryExecutionException(
            "Operation execution pipeline error. No terminal middleware used.",
            new NodePath());

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }
}