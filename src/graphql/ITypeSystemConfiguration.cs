namespace Tanka.GraphQL;

public interface ITypeSystemConfiguration
{
    Task Configure(SchemaBuilder schema, ResolversBuilder resolvers);
}