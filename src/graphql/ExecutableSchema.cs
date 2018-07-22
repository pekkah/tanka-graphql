using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fugu.graphql.introspection;
using fugu.graphql.resolvers;
using fugu.graphql.tools;
using fugu.graphql.type;

namespace fugu.graphql
{
    public class ExecutableSchema : ISchema
    {
        private readonly ISchema _schema;

        public ExecutableSchema(IResolverMap resolvers, ISubscriberMap subscribers, ISchema schema)
        {
            _schema = schema;
            Resolvers = resolvers;
            Subscribers = subscribers;
        }

        public ISubscriberMap Subscribers { get; }

        public IResolverMap Resolvers { get; }

        public bool IsInitialized => _schema.IsInitialized;

        public ObjectType Subscription => _schema.Subscription;

        public ObjectType Query => _schema.Query;

        public ObjectType Mutation => _schema.Mutation;

        public Task InitializeAsync()
        {
            return _schema.InitializeAsync();
        }

        public IGraphQLType GetNamedType(string name)
        {
            return _schema.GetNamedType(name);
        }

        public T GetNamedType<T>(string name) where T : IGraphQLType
        {
            return _schema.GetNamedType<T>(name);
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IGraphQLType
        {
            return _schema.QueryTypes(filter);
        }

        public DirectiveType GetDirective(string name)
        {
            return _schema.GetDirective(name);
        }

        public IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null)
        {
            return _schema.QueryDirectives(filter);
        }

        public static async Task<ExecutableSchema> MakeExecutableSchemaAsync(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            IEnumerable<SchemaVisitorFactory> visitors = null)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            if (visitors != null)
                foreach (var visitorFactory in visitors)
                {
                    var visitor = visitorFactory(schema, resolvers, subscribers);
                    await visitor.VisitAsync();
                }

            var executableSchema = new ExecutableSchema(resolvers, subscribers, schema);
            await executableSchema.InitializeAsync().ConfigureAwait(false);

            return executableSchema;
        }

        public static async Task<ExecutableSchema> MakeExecutableSchemaWithIntrospection(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            IEnumerable<SchemaVisitorFactory> visitors = null)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            if (visitors != null)
                foreach (var visitorFactory in visitors)
                {
                    var visitor = visitorFactory(schema, resolvers, subscribers);
                    await visitor.VisitAsync();
                }

            var introspection = await Introspection.ExamineAsync(schema);
            var withIntrospection = MergeTool.Merge(schema, introspection, FieldConflictResolver);
            var withIntrospectionResolvers = MergeTool.Merge(resolvers, introspection.Resolvers);

            return await MakeExecutableSchemaAsync(withIntrospection, withIntrospectionResolvers, subscribers);
        }

        private static IEnumerable<KeyValuePair<string, IField>> FieldConflictResolver(ComplexType left, ComplexType right, KeyValuePair<string, IField> conflict)
        {
            return new[] { conflict };
        }

        public Task<Resolver> GetResolverAsync(ResolverContext resolverContext)
        {
            if (Resolvers == null)
                throw new InvalidOperationException("Resolvers have not been set");

            return Resolvers.GetResolverAsync(resolverContext);
        }

        public Task<Subscriber> GetSubscriberAsync(ResolverContext resolverContext)
        {
            if (Resolvers == null)
                throw new InvalidOperationException("Resolvers have not been set");

            return Subscribers.GetSubscriberAsync(resolverContext);
        }
    }
}