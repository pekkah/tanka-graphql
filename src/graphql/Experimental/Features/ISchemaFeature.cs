using Tanka.GraphQL.Experimental.TypeSystem;

namespace Tanka.GraphQL.Experimental.Features;

public interface ISchemaFeature
{
    public ISchema Schema { get; set; }
}