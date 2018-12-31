using System;
using System.Collections.Generic;
using tanka.graphql.execution;
using tanka.graphql.type;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace tanka.graphql.resolvers
{
    public class ResolverContext
    {
        public ResolverContext(ObjectType objectType,
            object objectValue,
            IField field,
            GraphQLFieldSelection selection,
            Dictionary<string, object> arguments, 
            NodePath path)
        {
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            ObjectValue = objectValue;
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public ObjectType ObjectType { get; }

        public object ObjectValue { get; }

        public IField Field { get; }

        public GraphQLFieldSelection Selection { get; }

        public Dictionary<string, object> Arguments { get; }
        public NodePath Path { get; }

        public string FieldName => Selection.Name?.Value;

        public T GetArgument<T>(string name)
        {
            if (!Arguments.ContainsKey(name))
                return default(T);

            var arg = Arguments[name];

            if (arg is T argAsType)
                return argAsType;

            var obj = JObject.FromObject(arg);

            return obj.ToObject<T>();
        }
    }
}