using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fugu.graphql.type
{
    public class Schema : ISchema
    {
        public Schema(
            ObjectType query,
            ObjectType mutation = null,
            ObjectType subscription = null,
            IEnumerable<IGraphQLType> types = null,
            IEnumerable<DirectiveType> directives = null)
        {
            Query = query;
            Mutation = mutation;
            Subscription = subscription;

            if (types != null)
                AdditionalTypes.AddRange(types);

            if (directives != null)
                Directives.AddRange(directives);

            IsInitialized = false;
        }

        public Schema()
        {
            IsInitialized = false;
        }

        public IEnumerable<IGraphQLType> Types { get; protected set; }

        protected List<IGraphQLType> AdditionalTypes { get; } = new List<IGraphQLType>(ScalarType.Standard);

        public List<DirectiveType> Directives { get; } = new List<DirectiveType>
        {
            DirectiveType.Skip,
            DirectiveType.Include
        };

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

            foreach (var additionalType in AdditionalTypes)
                scanningTasks.Add(new TypeScanner(additionalType).ScanAsync());

            if (Mutation != null)
                scanningTasks.Add(new TypeScanner(Mutation).ScanAsync());

            if (Subscription != null)
                scanningTasks.Add(new TypeScanner(Subscription).ScanAsync());


            await Task.WhenAll(scanningTasks).ConfigureAwait(false);

            var foundTypes = scanningTasks.SelectMany(r => r.Result)
                .Distinct(new GraphQLTypeComparer())
                .ToList();

            Types = foundTypes;
            IsInitialized = true;
        }

        public IGraphQLType GetNamedType(string name)
        {
            return Types.SingleOrDefault(t => t.Name == name);
        }

        public T GetNamedType<T>(string name) where T : IGraphQLType
        {
            var type = GetNamedType(name);

            return (T) type;
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IGraphQLType
        {
            if (filter == null)
                return Types.OfType<T>().AsQueryable();

            return Types.OfType<T>().Where(t => filter(t)).AsQueryable();
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