using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tanka.GraphQL.Tests;

public  class OperationTest
{
    [Fact]
    public async Task AsyncStreamInContext()
    {
        /* Given */
        var pipeline = new Builder()
            .Use(_ => (context, token) =>
            {
                context.Response = Generate(token);
                return Task.CompletedTask;
            })
            .Build();

        /* When */
        var context = new Context();
        var cts = new CancellationTokenSource();
        await pipeline(context, cts.Token);

        /* Then */
        var all = await context.Response.ToListAsync(cts.Token);
        Assert.NotEmpty(all);
    }

    [Fact]
    public async Task With_Middleware_not_touching_response()
    {
        /* Given */
        var pipeline = new Builder()
            .Use(next => async (context, token) =>
            {
                token.ThrowIfCancellationRequested();
                await next(context, token);
            })
            .Use(_ => (context, token) =>
            {
                context.Response = Generate(token);
                return Task.CompletedTask;
            })
            .Build();

        /* When */
        var context = new Context();
        var cts = new CancellationTokenSource();
        await pipeline(context, cts.Token);

        /* Then */
        var all = await context.Response.ToListAsync(cts.Token);
        Assert.NotEmpty(all);
    }

    [Fact]
    public async Task With_Middleware_modify_Response()
    {
        /* Given */
        var pipeline = new Builder()
            .Use(next => async (context, token) =>
            {
                token.ThrowIfCancellationRequested();
                await next(context, token);

                context.Response = context.Response.Select(er =>
                {
                    er.Data = new Dictionary<string, object>()
                    {
                        ["key"] = "value"
                    };

                    return er;
                });
            })
            .Use(_ => (context, token) =>
            {
                context.Response = Generate(token);
                return Task.CompletedTask;
            })
            .Build();

        /* When */
        var context = new Context();
        var cts = new CancellationTokenSource();
        await pipeline(context, cts.Token);

        /* Then */
        var all = await context.Response.ToListAsync(cts.Token);
        Assert.NotEmpty(all);
        Assert.All(all, er => Assert.True(er.Data.ContainsKey("key")));
    }

    private async IAsyncEnumerable<ExecutionResult> Generate([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        yield return new ExecutionResult();
        yield return new ExecutionResult();
        yield return new ExecutionResult();

        await Task.Delay(0, cancellationToken);
    }
}

public class Context
{
    public IAsyncEnumerable<ExecutionResult> Response { get; set; }
}

public delegate Task Operation(Context Context, CancellationToken cancellationToken);

public class Builder
{

    private readonly List<Func<Operation, Operation>> _components = new();

    public Builder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IDictionary<string, object?> Properties { get; }

    public Operation Build()
    {
        Operation pipeline = (_, _) => throw new QueryException(
            "Operation execution pipeline error. No ending middleware.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }


    public Builder Use(Func<Operation, Operation> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    protected T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out object? value) ? (T?)value : default(T?);
    }

    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }
}