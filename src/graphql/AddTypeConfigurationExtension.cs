namespace Tanka.GraphQL;

public static class AddTypeConfigurationExtension
{
    public static ResolversBuilder AddTypeConfiguration(
        this ResolversBuilder builder,
        ITypeSystemConfiguration configuration)
    {
        configuration.Configure(builder);
        return builder;
    }

    public static SchemaBuilder AddTypeConfiguration(
        this SchemaBuilder builder,
        ITypeSystemConfiguration configuration)
    {
        configuration.Configure(builder);
        return builder;
    }
}