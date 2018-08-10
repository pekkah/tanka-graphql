using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fugu.graphql.type
{
    public class Schema : ISchema
    {
        private List<IGraphQLType> _types = new List<IGraphQLType>();

        public Schema(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<IGraphQLType> typesReferencedByNameOnly = null,
            IEnumerable<DirectiveType> directives = null)
        {
            // Query is required
            Query = query ?? throw new ArgumentNullException(nameof(query));

            // Mutation and subscription are optional
            Mutation = mutation;
            Subscription = subscription;

            IsInitialized = false;
        }

        public List<DirectiveType> Directives { get; } = new List<DirectiveType>();

        public bool IsInitialized { get; protected set; }

        public ObjectType Subscription { get; protected set; }

        public ObjectType Query { get; protected set; }

        public ObjectType Mutation { get; protected set; }

        public virtual async Task InitializeAsync()
        {
            if (IsInitialized)
                return;

            var scanningTasks = new List<Task<IEnumerable<IGraphQLType>>>
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
                .Distinct(new GraphQLTypeComparer())
                .ToList();

            // add default directives
            Directives.Add(DirectiveType.Include);
            Directives.Add(DirectiveType.Skip);

            _types = foundTypes;
            IsInitialized = true;
        }

        public IGraphQLType GetNamedType(string name)
        {
            return _types.SingleOrDefault(t => t.Name == name);
        }

        public T GetNamedType<T>(string name) where T : IGraphQLType
        {
            var type = GetNamedType(name);

            return (T) type;
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IGraphQLType
        {
            if (filter == null)
                return _types.OfType<T>().AsQueryable();

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
    }
}