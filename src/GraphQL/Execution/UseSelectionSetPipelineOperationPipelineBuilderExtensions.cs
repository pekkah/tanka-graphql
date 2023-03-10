using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL.Execution;

public static class SelectionSetExecutorOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder AddSelectionSetPipeline(
        this OperationDelegateBuilder builder,
        Action<SelectionSetPipelineBuilder> configurePipeline)
    {
        var selectionSetPipelineBuilder = new SelectionSetPipelineBuilder(builder.ApplicationServices);
        configurePipeline(selectionSetPipelineBuilder);

        var feature = new SelectionSetDelegateExecutorFeature(selectionSetPipelineBuilder.Build());

        builder.Use(next => context =>
        {
            context.Features.Set<ISelectionSetExecutorFeature>(feature);
            return next(context);
        });

        return builder;
    }
}