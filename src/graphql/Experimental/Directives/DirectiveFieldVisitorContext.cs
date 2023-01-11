using System;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.Directives;

public record DirectiveFieldVisitorContext(
    FieldDefinition Field,
    Resolver? Resolver,
    Subscriber? Subscriber)
{
    public DirectiveFieldVisitorContext WithResolver(
        Action<ResolverBuilder> build)
    {
        if (build == null) throw new ArgumentNullException(nameof(build));

        var builder = new ResolverBuilder();
        build(builder);

        return this with
        {
            Resolver = builder.Build()
        };
    }

    public DirectiveFieldVisitorContext WithSubscriber(
        Action<ResolverBuilder> buildResolver,
        Action<SubscriberBuilder> buildSubscriber)
    {
        if (buildResolver == null) throw new ArgumentNullException(nameof(buildResolver));
        if (buildSubscriber == null) throw new ArgumentNullException(nameof(buildSubscriber));

        var resolverBuilder = new ResolverBuilder();
        buildResolver(resolverBuilder);

        var subscriberBuilder = new SubscriberBuilder();
        buildSubscriber(subscriberBuilder);

        return this with
        {
            Resolver = resolverBuilder.Build(),
            Subscriber = subscriberBuilder.Build()
        };
    }
}