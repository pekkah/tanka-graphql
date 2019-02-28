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

        IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : INamedType;

        DirectiveType GetDirective(string name);

        IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null);

        IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(string type);

        InputObjectField GetInputField(string type, string name);

        IEnumerable<ObjectType> GetPossibleTypes(IAbstractType abstractType);
    }
}