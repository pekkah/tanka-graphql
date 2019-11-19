using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public interface IResolverContext
    {
        ObjectType ObjectType { get; }

        object ObjectValue { get; }

        IField Field { get; }

        GraphQLFieldSelection Selection { get; }

        IReadOnlyDictionary<string, object> Arguments { get; }

        NodePath Path { get; }

        IExecutorContext ExecutionContext { get; }

        string FieldName { get; }

        T GetArgument<T>(string name);

        /// <summary>
        ///     Read InputObject argument dictionary as object
        /// </summary>
        /// <remarks>
        ///     Experimental. This might go away anytime and be replaced with something better.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetObjectArgument<T>(string name)
            where T : IReadFromObjectDictionary, new();

        T Extension<T>() where T : IExtensionScope;
    }
}