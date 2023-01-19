using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public class SchemaOptions
{
    public string SchemaName { get; set; } = string.Empty;

    public ExecutableSchemaBuilder Builder = new();
}