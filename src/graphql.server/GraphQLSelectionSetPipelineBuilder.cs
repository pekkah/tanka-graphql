using System;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL.Server;

public class GraphQLSelectionSetPipelineBuilder:SelectionSetPipelineBuilder
{
    public GraphQLSelectionSetPipelineBuilder(
        IServiceProvider applicationServices)
    {
        ApplicationServices = applicationServices;
    }

    public IServiceProvider ApplicationServices { get; }
}