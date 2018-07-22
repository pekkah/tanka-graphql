using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class Field : IField
    {
        private readonly Args _arguments = new Args();

        public Field(
            IGraphQLType type,
            Args arguments = null,
            Meta meta = null,
            object defaultValue = null)
        {
            Type = type;
            Meta = meta ?? new Meta();
            DefaultValue = defaultValue;

            if (arguments != null)
                foreach (var argument in arguments)
                    _arguments[argument.Key] = argument.Value;
        }

        public object DefaultValue { get; set; }

        public Meta Meta { get; set; }

        public IGraphQLType Type { get; }

        public IEnumerable<KeyValuePair<string, Argument>> Arguments => _arguments;

        public IEnumerable<DirectiveInstance> Directives => Meta.Directives;

        public DirectiveInstance GetDirective(string name)
        {
            return Meta.GetDirective(name);
        }

        public Argument GetArgument(string name)
        {
            if (!_arguments.ContainsKey(name))
                return null;

            return _arguments[name];
        }

        public bool HasArgument(string name)
        {
            return _arguments.ContainsKey(name);
        }
    }
}