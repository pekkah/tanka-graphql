using System;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Directives;

public class DirectiveFieldVisitorContext : IEquatable<DirectiveFieldVisitorContext>
{
    public DirectiveFieldVisitorContext(
        FieldDefinition value,
        Resolver resolver,
        Subscriber subscriber)
    {
        Field = value;
        Resolver = resolver;
        Subscriber = subscriber;
    }

    public FieldDefinition Field { get; }

    public Resolver Resolver { get; }

    public Subscriber Subscriber { get; }

    public bool Equals(DirectiveFieldVisitorContext? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Field.Equals(other.Field) && Resolver.Equals(other.Resolver) && Subscriber.Equals(other.Subscriber);
    }

    public DirectiveFieldVisitorContext WithResolver(Action<ResolverBuilder> build)
    {
        if (build == null) throw new ArgumentNullException(nameof(build));

        var builder = new ResolverBuilder();
        build(builder);

        return new DirectiveFieldVisitorContext(Field, builder.Build(), Subscriber);
    }

    public DirectiveFieldVisitorContext WithSubscriber(Action<ResolverBuilder> buildResolver,
        Action<SubscriberBuilder> buildSubscriber)
    {
        if (buildResolver == null) throw new ArgumentNullException(nameof(buildResolver));
        if (buildSubscriber == null) throw new ArgumentNullException(nameof(buildSubscriber));

        var resolverBuilder = new ResolverBuilder();
        buildResolver(resolverBuilder);

        var subscriberBuilder = new SubscriberBuilder();
        buildSubscriber(subscriberBuilder);

        return new DirectiveFieldVisitorContext(Field, resolverBuilder.Build(), subscriberBuilder.Build());
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DirectiveFieldVisitorContext)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Field, Resolver, Subscriber);
    }

    public static bool operator ==(DirectiveFieldVisitorContext? left, DirectiveFieldVisitorContext? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DirectiveFieldVisitorContext? left, DirectiveFieldVisitorContext? right)
    {
        return !Equals(left, right);
    }
}