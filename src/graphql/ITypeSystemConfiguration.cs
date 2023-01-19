namespace Tanka.GraphQL;

public interface IExecutableSchemaConfiguration
{
    Task Configure(SchemaBuilder schema, ResolversBuilder resolvers);
}