using System;
using System.Linq;
using System.Threading.Tasks;

namespace tanka.graphql.type
{
    public interface ISchema
    {
        bool IsInitialized { get; }

        ObjectType Subscription { get; }

        ObjectType Query { get; }

        ObjectType Mutation { get; }

        Task InitializeAsync();

        IGraphQLType GetNamedType(string name);

        T GetNamedType<T>(string name) where T : IGraphQLType;

        IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IGraphQLType;

        DirectiveType GetDirective(string name);

        IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null);
    }
}