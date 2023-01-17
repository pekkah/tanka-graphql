namespace Tanka.GraphQL.Features;

public class SchemaFeature : ISchemaFeature
{
    public required ISchema Schema { get; set; }
}