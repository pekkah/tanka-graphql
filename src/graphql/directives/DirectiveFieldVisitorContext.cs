using System;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.directives
{
    public class DirectiveFieldVisitorContext: IEquatable<DirectiveFieldVisitorContext>
    {
        public DirectiveFieldVisitorContext(string name, IField value, Resolver resolver,
            Subscriber subscriber)
        {
            Name = name;
            Field = value;
            Resolver = resolver;
            Subscriber = subscriber;
        }

        public string Name { get; }

        public IField Field { get; }

        public Resolver Resolver { get; }

        public Subscriber Subscriber { get; }

        public DirectiveFieldVisitorContext WithResolver(Action<ResolverBuilder> build)
        {
            if (build == null) throw new ArgumentNullException(nameof(build));

            var builder = new ResolverBuilder();
            build(builder);

            return new DirectiveFieldVisitorContext(Name,Field, builder.Build(), Subscriber);
        }

        public DirectiveFieldVisitorContext WithSubscriber(Action<ResolverBuilder> buildResolver, Action<SubscriberBuilder> buildSubscriber)
        {
            if (buildResolver == null) throw new ArgumentNullException(nameof(buildResolver));
            if (buildSubscriber == null) throw new ArgumentNullException(nameof(buildSubscriber));

            var resolverBuilder = new ResolverBuilder();
            buildResolver(resolverBuilder);

            var subscriberBuilder = new SubscriberBuilder();
            buildSubscriber(subscriberBuilder);

            return new DirectiveFieldVisitorContext(Name,Field, resolverBuilder.Build(), subscriberBuilder.Build());
        }

        public bool Equals(DirectiveFieldVisitorContext other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(Field, other.Field) && Equals(Resolver, other.Resolver) && Equals(Subscriber, other.Subscriber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DirectiveFieldVisitorContext) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Field != null ? Field.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Resolver != null ? Resolver.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Subscriber != null ? Subscriber.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DirectiveFieldVisitorContext left, DirectiveFieldVisitorContext right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DirectiveFieldVisitorContext left, DirectiveFieldVisitorContext right)
        {
            return !Equals(left, right);
        }
    }
}