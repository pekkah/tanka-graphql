using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.graph;

namespace tanka.graphql.type
{
    public class Schema : ISchema
    {
        private readonly IEnumerable<INamedType> _typesReferencedByNameOnly;
        private List<IType> _types = new List<IType>();

        [Obsolete("Use Schema.Initialize")]
        public Schema(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<INamedType> typesReferencedByNameOnly = null,
            IEnumerable<DirectiveType> directives = null)
        {
            _typesReferencedByNameOnly = typesReferencedByNameOnly;
            // Query is required
            Query = query ?? throw new ArgumentNullException(nameof(query));

            // Mutation and subscription are optional
            Mutation = mutation;
            Subscription = subscription;

            if (directives != null)
                Directives.AddRange(directives);

            IsInitialized = false;
        }

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
            Directives.AddRange(directives);

            var namedTypes = _types.OfType<ObjectType>()
                .ToList();

            Query = namedTypes.Single(t => t.Name == queryTypeName);

            if (!string.IsNullOrEmpty(mutationTypeName))
                Mutation = namedTypes.Single(t => t.Name == mutationTypeName);

            if (!string.IsNullOrEmpty(subscriptionTypeName))
                Subscription = namedTypes.Single(t => t.Name == subscriptionTypeName);
        }

        public List<DirectiveType> Directives { get; } = new List<DirectiveType>();

        public bool IsInitialized { get; protected set; }

        public ObjectType Subscription { get; protected set; }

        public ObjectType Query { get; protected set; }

        public ObjectType Mutation { get; protected set; }

        [Obsolete]
        public virtual async Task<ISchema> InitializeAsync()
        {
            if (IsInitialized)
                return this;

            var scanningTasks = new List<Task<IEnumerable<IType>>>
            {
                new TypeScanner(Query).ScanAsync()
            };

            if (Mutation != null)
                scanningTasks.Add(new TypeScanner(Mutation).ScanAsync());

            if (Subscription != null)
                scanningTasks.Add(new TypeScanner(Subscription).ScanAsync());


            await Task.WhenAll(scanningTasks).ConfigureAwait(false);

            // combine
            var foundTypes = scanningTasks.SelectMany(r => r.Result)
                //.Concat(ScalarType.Standard) // disabled for now
                .Concat(_typesReferencedByNameOnly ?? Enumerable.Empty<IType>())
                .Distinct(new GraphQLTypeComparer())
                .ToList();

            // add default directives
            Directives.Add(DirectiveType.Include);
            Directives.Add(DirectiveType.Skip);

            _types = foundTypes;

            var heal = new SchemaHealer(this);
            await heal.VisitAsync();

            IsInitialized = true;
            return this;
        }

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

        public static ISchema Initialize(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<INamedType> byNameOnly = null,
            IEnumerable<DirectiveType> directiveTypes = null,
            params SchemaTransform[] transforms)
        {
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
            var directives = new List<DirectiveType>(
                directiveTypes ?? Enumerable.Empty<DirectiveType>()
            )
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

        public T GetNamedType<T>(string name) where T : INamedType
        {
            var type = GetNamedType(name);

            return (T) type;
        }
    }
}