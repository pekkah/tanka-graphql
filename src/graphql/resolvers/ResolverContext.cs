using System;
using System.Collections.Generic;
using fugu.graphql.type;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace fugu.graphql.resolvers
{
    public class ResolverContext
    {
        public ResolverContext(
            ObjectType objectType,
            object objectValue,
            IField field,
            GraphQLFieldSelection selection,
            Dictionary<string, object> arguments)
        {
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
            ObjectValue = objectValue;
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            Arguments = arguments;
        }

        public ObjectType ObjectType { get; }

        public object ObjectValue { get; }

        public IField Field { get; }

        public GraphQLFieldSelection Selection { get; }

        public Dictionary<string, object> Arguments { get; }

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