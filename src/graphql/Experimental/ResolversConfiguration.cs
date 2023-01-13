using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public class ResolversConfiguration : ITypeSystemConfiguration
{
    private readonly IResolverMap _resolversMap;

    public ResolversConfiguration(IResolverMap resolversMap)
    {
        _resolversMap = resolversMap;
    }

    public Task Configure(TypeSystem.SchemaBuilder schema, ResolversBuilder resolvers)
    {
        foreach (var (typeName, fields) in _resolversMap.GetTypes())
        {
            foreach (var field in fields)
            {
                var resolver = _resolversMap.GetResolver(typeName, field);
                if (resolver is not null)
                    resolvers.Add(typeName, field, r => r.Run(resolver));
            }
        }

        return Task.CompletedTask;
    }
}