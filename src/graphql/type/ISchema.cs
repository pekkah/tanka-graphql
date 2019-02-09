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

        Task<ISchema> InitializeAsync();

        ISchema Initialize();

        INamedType GetNamedType(string name);

        //T GetNamedType<T>(string name) where T : INamedType;

        IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IType;

        DirectiveType GetDirective(string name);

        IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null);
    }
}