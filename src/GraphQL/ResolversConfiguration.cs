namespace Tanka.GraphQL;

public class ResolversConfiguration : IExecutableSchemaConfiguration
{
    private readonly IResolverMap _resolversMap;

    public ResolversConfiguration(IResolverMap resolversMap)
    {
        _resolversMap = resolversMap;
    }

    public Task Configure(SchemaBuilder schema, ResolversBuilder resolvers)
    {
        foreach (var (typeName, fields) in _resolversMap.GetTypes())
        foreach (var field in fields)
        {
            var resolver = _resolversMap.GetResolver(typeName, field);
            if (resolver is not null)
                resolvers.Add(typeName, field, r => r.Run(resolver));
        }

        return Task.CompletedTask;
    }
}