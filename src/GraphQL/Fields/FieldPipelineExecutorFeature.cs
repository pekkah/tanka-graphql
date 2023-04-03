using System.Collections.Immutable;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public delegate ValueTask FieldDelegate(ResolverContext context);

public class FieldPipelineExecutorFeature : IFieldExecutorFeature
{
    private readonly FieldDelegate _fieldDelegate;

    public FieldPipelineExecutorFeature(FieldDelegate fieldDelegate)
    {
        _fieldDelegate = fieldDelegate;
    }

    public async Task<object?> Execute(
        QueryContext context, 
        ObjectDefinition objectDefinition, 
        object? objectValue, 
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        var fieldSelection = fields.First();
        var resolverContext = new ResolverContext
        {
            ObjectDefinition = objectDefinition,
            ObjectValue = objectValue,
            Field = context.Schema.GetField(objectDefinition.Name.Value, fieldSelection.Name.Value),
            Selection = fieldSelection,
            Fields = fields,
            ArgumentValues = ImmutableDictionary<string, object?>.Empty,
            Path = path.Append(fieldSelection.Name.Value),
            QueryContext = context
        };

        await _fieldDelegate(resolverContext);

        return resolverContext.CompletedValue;
    }
}