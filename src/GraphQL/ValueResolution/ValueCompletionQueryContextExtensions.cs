using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueResolution;

public static class ValueCompletionQueryContextExtensions
{
    public static ValueTask CompleteValueAsync(
        this QueryContext queryContext,
        ResolverContext context,
        TypeBase fieldType,
        NodePath path)
    {
        return queryContext.ValueCompletionFeature.CompleteValueAsync(context, fieldType, path);
    }
}