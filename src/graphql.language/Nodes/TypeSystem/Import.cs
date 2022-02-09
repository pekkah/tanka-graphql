using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class Import : INode
{
    public Import(
        IReadOnlyList<Name>? types,
        StringValue from,
        in Location? location = default)
    {
        Types = types;
        From = from;
        Location = location;
    }

    public StringValue From { get; }
    public IReadOnlyList<Name>? Types { get; }

    public NodeKind Kind => NodeKind.TankaImport;
    public Location? Location { get; }

    public override string ToString()
    {
        var types = string.Empty;

        if (Types is not null)
            types = $" {string.Join(',', Types.Select(t => t.Value))}";

        return $"tanka_import{types} from \"{From}\"";
    }
}