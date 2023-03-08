using Tanka.GraphQL.Features;

namespace Tanka.GraphQL.TypeSystem;

public class SchemaFeature : ISchemaFeature
{
    public ISchema Schema { get; set; } = ISchema.Empty;
}