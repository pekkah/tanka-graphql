namespace Tanka.GraphQL;

public interface ITypeSystemConfiguration
{
    void Configure(ResolversBuilder builder);

    void Configure(SchemaBuilder builder);
}