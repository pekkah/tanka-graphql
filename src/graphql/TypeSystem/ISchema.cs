using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem
{
    public interface ISchema : IHasDirectives
    {
        ObjectType Subscription { get; }

        ObjectType Query { get; }

        ObjectType Mutation { get; }

        INamedType GetNamedType(string name);

        IField GetField(string type, string name);

        IEnumerable<KeyValuePair<string, IField>> GetFields(string type);

        IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : INamedType;

        DirectiveType GetDirectiveType(string name);

        IQueryable<DirectiveType> QueryDirectiveTypes(Predicate<DirectiveType> filter = null);

        IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(string type);

        InputObjectField GetInputField(string type, string name);

        IEnumerable<ObjectType> GetPossibleTypes(IAbstractType abstractType);

        Resolver GetResolver(string type, string fieldName);

        Subscriber GetSubscriber(string type, string fieldName);

        IValueConverter GetScalarSerializer(string type);
    }
}