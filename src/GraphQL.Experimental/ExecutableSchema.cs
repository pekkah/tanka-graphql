using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public class ExecutableSchema
    {
        private readonly IReadOnlyDictionary<string, TypeDefinition> _typeDefinitions;

        public ExecutableSchema(
            ObjectDefinition queryRoot,
            ObjectDefinition? mutationRoot,
            ObjectDefinition? subscriptionRoot,
            IReadOnlyDictionary<string, TypeDefinition> typeDefinitions)
        {
            Query = queryRoot;
            Mutation = mutationRoot;
            Subscription = subscriptionRoot;
            _typeDefinitions = typeDefinitions;
        }

        public ObjectDefinition? Subscription { get; }

        public ObjectDefinition Query { get; }

        public ObjectDefinition? Mutation { get; }

        public T? GetNamedType<T>(string name) where T : TypeDefinition
        {
            if (_typeDefinitions.TryGetValue(name, out var typeDefinition)) return (T) typeDefinition;

            return null;
        }

        public static implicit operator ExecutableSchema(TypeSystemDocument typeSystem)
        {
            return new ExecutableSchemaBuilder()
                .Add(typeSystem)
                .Build();
        }
    }
}