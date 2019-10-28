using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public class ResolverContext
    {
        public ResolverContext(
            ISchema schema, //todo: remove and get from execution
            ObjectType objectType,
            object objectValue,
            IField field,
            GraphQLFieldSelection selection,
            IReadOnlyDictionary<string, object> arguments,
            NodePath path,
            IExecutorContext executionContext)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            ObjectValue = objectValue;
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ExecutionContext = executionContext;
        }

        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public ISchema Schema { get; }

        public ObjectType ObjectType { get; }

        public object ObjectValue { get; }

        public IField Field { get; }

        public GraphQLFieldSelection Selection { get; }

        public IReadOnlyDictionary<string, object> Arguments { get; }

        public NodePath Path { get; }

        public IExecutorContext ExecutionContext { get; }

        public string FieldName => Selection.Name?.Value;

        public T GetArgument<T>(string name)
        {
            if (!Arguments.TryGetValue(name, out var arg))
                throw new ArgumentOutOfRangeException(nameof(name), name,
                    $"Field '{FieldName}' does not contain argument with name '{name}''");

            return (T) arg;
        }

        /// <summary>
        ///     Read InputObject argument dictionary as object
        /// </summary>
        /// <remarks>
        ///     Experimental. This might go away anytime and be replaced with something better.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetObjectArgument<T>(string name)
            where T : IReadFromObjectDictionary, new()
        {
            var arg = GetArgument<IReadOnlyDictionary<string, object>>(name);

            var value = new T();
            value.Read(arg);
            return value;
        }

        public T Extension<T>() where T : IExtensionScope
        {
            return ExecutionContext.ExtensionsRunner.Extension<T>();
        }
    }


    public interface IReadFromObjectDictionary
    {
        void Read(IReadOnlyDictionary<string, object> source);
    }
}