using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Internal;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public class ResolverBuilder
{
    private readonly List<Func<Resolver, Resolver>> _components = new();

    protected ResolverBuilder(ResolverBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected ResolverBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public ResolverBuilder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IDictionary<string, object?> Properties { get; }

    public ResolverBuilder New()
    {
        return new ResolverBuilder(this);
    }

    public ResolverBuilder Use(Func<Resolver, Resolver> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public ResolverBuilder Use(Func<IResolverContext, Resolver, ValueTask<IResolverResult>> middleware)
    {
        return Use(next => context => middleware(context, next));
    }

    public ResolverBuilder Run(Resolver end)
    {
        return Use(_ => end);
    }

    public ResolverBuilder Prepend(Func<Resolver, Resolver> middleware)
    {
        _components.Insert(0, middleware);
        return this;
    }

    public Resolver Build()
    {
        Resolver pipeline = context => throw new QueryExecutionException(
            $"Selection {Printer.Print(context.Selection)} d.",
            context.Path, 
            context.Field);

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    protected T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out var value) ? (T?)value : default;
    }

    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }
}