using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Features;

public class OperationFeature : IOperationFeature
{
    public required OperationDefinition Operation { get; set; }
}