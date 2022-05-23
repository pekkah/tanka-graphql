using System;
using System.Collections.Generic;
using Tanka.GraphQL.Internal;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public class SubscriberBuilder
{
    private readonly List<Func<Subscriber, Subscriber>> _components = new();

    protected SubscriberBuilder(SubscriberBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected SubscriberBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public SubscriberBuilder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IDictionary<string, object?> Properties { get; }

    public SubscriberBuilder New()
    {
        return new SubscriberBuilder(this);
    }

    public SubscriberBuilder Use(Func<Subscriber, Subscriber> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public Subscriber Build()
    {
        Subscriber pipeline = (context, _) => throw new QueryExecutionException(
            $"Selection {Printer.Print(context.Selection)} did not subscribe any value.",
            context.Path, context.Field);

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