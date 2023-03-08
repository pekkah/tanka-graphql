using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Execution;

public static class OperationTypeOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder WhenOperationTypeIsUse(
        this OperationDelegateBuilder builder,
        Action<OperationDelegateBuilder> queryAction,
        Action<OperationDelegateBuilder> mutationAction,
        Action<OperationDelegateBuilder> subscriptionAction)
    {
        var queryBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        queryAction(queryBuilder);
        OperationDelegate queryDelegate = queryBuilder.Build();

        var mutationBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        mutationAction(mutationBuilder);
        OperationDelegate mutationDelegate = mutationBuilder.Build();

        var subscriptionBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        subscriptionAction(subscriptionBuilder);
        OperationDelegate subscriptionDelegate = subscriptionBuilder.Build();

        return builder.Use(_ => async context =>
        {
            Task execute = context.OperationDefinition.Operation switch
            {
                OperationType.Query => queryDelegate(context),
                OperationType.Mutation => mutationDelegate(context),
                OperationType.Subscription => subscriptionDelegate(context),
                _ => throw new InvalidOperationException(
                    $"Operation type {context.OperationDefinition.Operation} not supported.")
            };

            await execute;
        });
    }
}