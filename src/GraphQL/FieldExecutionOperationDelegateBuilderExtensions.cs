namespace Tanka.GraphQL;

public static class FieldExecutionOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder AddDefaultFieldExecutorFeature(
        this OperationDelegateBuilder builder)
    {
        var feature = new FieldExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IFieldExecutorFeature>(feature);
            return next(context);
        });
    }

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