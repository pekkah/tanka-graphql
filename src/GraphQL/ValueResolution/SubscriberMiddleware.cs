namespace Tanka.GraphQL.ValueResolution;

public delegate ValueTask SubscriberMiddleware(
    SubscriberContext context,
    CancellationToken unsubscribe,
    Subscriber next);