using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.graph;

namespace tanka.graphql.type
{
    public class Schema : ISchema
    {
        private readonly List<IType> _types = new List<IType>();
        private readonly List<DirectiveType> _directives = new List<DirectiveType>();

        protected Schema(
            string queryTypeName,
            string mutationTypeName,
            string subscriptionTypeName,
            IEnumerable<IType> types,
            IEnumerable<DirectiveType> directives)
        {
            if (queryTypeName == null) throw new ArgumentNullException(nameof(queryTypeName));
            if (types == null) throw new ArgumentNullException(nameof(types));

            _types.AddRange(types);
            _directives.AddRange(directives);

            var namedTypes = _types.OfType<ObjectType>()
                .ToList();

            Query = namedTypes.SingleOrDefault(t => t.Name == queryTypeName) 
                    ?? throw new ArgumentNullException("Query is required for schema " +
                                                       $"Could not find type '{queryTypeName}'");

            if (!string.IsNullOrEmpty(mutationTypeName))
                Mutation = namedTypes.Single(t => t.Name == mutationTypeName);

            if (!string.IsNullOrEmpty(subscriptionTypeName))
                Subscription = namedTypes.Single(t => t.Name == subscriptionTypeName);

            IsInitialized = true;
        }

        public IEnumerable<DirectiveType> Directives => _directives;

        public bool IsInitialized { get; protected set; }

        public ObjectType Subscription { get; protected set; }

        public ObjectType Query { get; protected set; }

        public ObjectType Mutation { get; protected set; }

        public INamedType GetNamedType(string name)
        {
            return _types.OfType<INamedType>()
                .Where(type => !(type is DirectiveType))
                .SingleOrDefault(t => t.Name == name);
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IType
        {
            if (filter == null)
                return _types.OfType<T>()
                    .Where(type => !(type is DirectiveType))
                    .AsQueryable();

            return _types.OfType<T>().Where(t => filter(t)).AsQueryable();
        }

        public virtual DirectiveType GetDirective(string name)
        {
            return Directives.FirstOrDefault(d => d.Name == name);
        }

        public IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null)
        {
            if (filter == null)
                return Directives.AsQueryable();

            return Directives.Where(d => filter(d)).AsQueryable();
        }

        /// <summary>
        ///     Initialize schema and replace NamedTypeReferences
        ///     with actual types
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="mutation">Mutation</param>
        /// <param name="subscription">Subscription</param>
        /// <param name="byNameOnly">Types referenced by name only</param>
        /// <param name="directiveTypes">Directives</param>
        /// <returns>Initialized <see cref="ISchema" /></returns>
        public static ISchema Initialize(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<INamedType> byNameOnly = null,
            IEnumerable<DirectiveType> directiveTypes = null)
        {
            return Initialize(
                query,
                mutation,
                subscription,
                byNameOnly,
                directiveTypes,
                Transforms.Heal());
        }

        /// <summary>
        ///     Initialize schema
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="mutation">Mutation</param>
        /// <param name="subscription">Subscription</param>
        /// <param name="byNameOnly">Types referenced by name only</param>
        /// <param name="directiveTypes">Directives</param>
        /// <param name="transforms">Schema transformations</param>
        /// <returns>Initialized <see cref="ISchema" /></returns>
        public static ISchema Initialize(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<INamedType> byNameOnly = null,
            IEnumerable<DirectiveType> directiveTypes = null,
            params SchemaTransform[] transforms)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var scanningTasks = new List<Task<IEnumerable<IType>>>
            {
                new TypeScanner(query).ScanAsync()
            };

            if (mutation != null)
                scanningTasks.Add(new TypeScanner(mutation).ScanAsync());

            if (subscription != null)
                scanningTasks.Add(new TypeScanner(subscription).ScanAsync());

            Task.WhenAll(scanningTasks).ConfigureAwait(false)
                .GetAwaiter().GetResult();

            // combine
            var foundTypes = scanningTasks.SelectMany(r => r.Result)
                //.Concat(ScalarType.Standard) // disabled for now
                .Concat(byNameOnly ?? Enumerable.Empty<IType>())
                .Distinct(new GraphQLTypeComparer())
                .ToList();

            // add default directives
            var directives = directiveTypes ?? new List<DirectiveType>()
            {
                DirectiveType.Include,
                DirectiveType.Skip
            };

            var schema = new Schema(
                query.Name,
                mutation?.Name,
                subscription?.Name,
                foundTypes,
                directives);

            return Transforms.Apply(schema, transforms.ToArray());
        }
    }
}