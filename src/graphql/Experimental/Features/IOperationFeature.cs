using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental.Features;

public interface IOperationFeature
{
    public OperationDefinition Operation { get; set; }
}