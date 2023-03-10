using Tanka.GraphQL.Features;

namespace Tanka.GraphQL.Execution;

public static class FieldExecutionOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder AddFieldDelegateExecutorFeature(
        this OperationDelegateBuilder builder,
        Action<FieldDelegateBuilder> configureFieldExecutor)
    {
        var fieldDelegateBuilder = new FieldDelegateBuilder(builder.ApplicationServices);
        configureFieldExecutor(fieldDelegateBuilder);

        var feature = new FieldPipelineExecutorFeature(fieldDelegateBuilder.Build());
        return builder.Use(next => context =>
        {
            context.Features.Set<IFieldExecutorFeature>(feature);
            return next(context);
        });
    }
}