namespace Tanka.GraphQL;

public delegate ValueTask SubscriberMiddleware(
    SubscriberContext context,
    CancellationToken unsubscribe,
    Subscriber next);