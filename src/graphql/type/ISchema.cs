using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public interface ISchema
    {
        bool IsInitialized { get; }

        ObjectType Subscription { get; }

        ObjectType Query { get; }

        ObjectType Mutation { get; }

        INamedType GetNamedType(string name);

        IField GetField(string type, string name);

        IEnumerable<KeyValuePair<string, IField>> GetFields(string type);

        IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IType;

        DirectiveType GetDirective(string name);

        IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null);
    }

    public static class SchemaExtensions
    {
        public static T GetNamedType<T>(this ISchema schema, string name) where T: INamedType
        {
            return (T) schema.GetNamedType(name);
        }
    }
}