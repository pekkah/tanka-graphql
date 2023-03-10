using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Executable;

public interface IExecutableSchemaConfiguration
{
    Task Configure(SchemaBuilder schema, ResolversBuilder resolvers);
}