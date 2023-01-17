using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Features;

public interface IOperationFeature
{
    public OperationDefinition Operation { get; set; }
}