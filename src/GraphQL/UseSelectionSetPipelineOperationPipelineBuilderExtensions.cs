using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL;

public static class UseSelectionSetPipelineOperationPipelineBuilderExtensions
{
    public static OperationPipelineBuilder AddSelectionSetPipeline(
        this OperationPipelineBuilder builder,
        Action<SelectionSetPipelineBuilder> configurePipeline)
    {
        var selectionSetPipelineBuilder = new SelectionSetPipelineBuilder(builder.ApplicationServices);
        configurePipeline(selectionSetPipelineBuilder);

        var feature = new SelectionSetPipelineExecutorFeature(selectionSetPipelineBuilder.Build());

        builder.Use(next => context =>
        {
            context.Features.Set<ISelectionSetExecutorFeature>(feature);
            return next(context);
        });

        return builder;
    }
}