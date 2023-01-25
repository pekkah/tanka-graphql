namespace Tanka.GraphQL.ValueResolution;

public delegate ValueTask Subscriber(
    SubscriberContext context,
    CancellationToken unsubscribe);