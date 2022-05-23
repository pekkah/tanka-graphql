using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Server;

public class SchemaOptions
{
    public string SchemaName { get; set; } = string.Empty;

    public List<string> HttpLinks { get; } = new();

    public List<ITypeSystemConfiguration> Configurations { get; } = new();

    public List<TypeSystemDocument> Documents { get; } = new();

    public List<Func<SchemaBuilder, Task>> ConfigureSchema { get; } = new();

    public List<Func<ResolversBuilder, Task>> ConfigureResolvers { get; } = new();

}