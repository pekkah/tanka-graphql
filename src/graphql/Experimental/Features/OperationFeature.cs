using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental.Features;

public class OperationFeature : IOperationFeature
{
    public required OperationDefinition Operation { get; set; }
}