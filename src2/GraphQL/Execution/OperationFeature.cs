using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Execution;

public class OperationFeature : IOperationFeature
{
    public OperationFeature()
    {
        Operation = Empty;
    }

    public static OperationDefinition Empty = "{}";

    public OperationDefinition Operation { get; set; }
}