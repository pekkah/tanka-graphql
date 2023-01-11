using Tanka.GraphQL.Experimental.TypeSystem;

namespace Tanka.GraphQL.Experimental.Features;

public class SchemaFeature : ISchemaFeature
{
    public required ISchema Schema { get; set; }
}