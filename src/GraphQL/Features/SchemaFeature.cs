namespace Tanka.GraphQL.Features;

public class SchemaFeature : ISchemaFeature
{
    public ISchema Schema { get; set; } = ISchema.Empty;
}