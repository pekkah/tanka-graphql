namespace Tanka.GraphQL.ValueResolution;

public class ResolverBuilder
{
    private readonly List<Func<Resolver, Resolver>> _components = new();

    public ResolverBuilder Use(Func<Resolver, Resolver> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public ResolverBuilder Run(Resolver resolver)
    {
        return Use(_ => resolver);
    }

    public ResolverBuilder Run(Delegate resolver)
    {
        return Use(_ => DelegateResolverFactory.GetOrCreate(resolver));
    }

    public Resolver Build()
    {
        Resolver resolver = context => ValueTask.CompletedTask;

        for (int c = _components.Count - 1; c >= 0; c--)
            resolver = _components[c](resolver);

        return resolver;
    }
}