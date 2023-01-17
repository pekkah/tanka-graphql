namespace Tanka.GraphQL;

public delegate ValueTask Subscriber(
    SubscriberContext context,
    CancellationToken unsubscribe);