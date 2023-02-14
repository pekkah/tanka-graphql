using System.Collections.Generic;

namespace Tanka.GraphQL.Server;

public class GraphQLApplicationOptions
{
    public List<string> SchemaNames { get; } = new();

    public bool EnableUi { get; set; } = true;
}