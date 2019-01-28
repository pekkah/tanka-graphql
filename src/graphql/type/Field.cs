using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public class Field : IField
    {
        private readonly Args _arguments = new Args();

        public Field(
            IType type,
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

        public Resolver Resolve { get; set; }

        public Subscriber Subscribe { get; set; }

        public IType Type { get; set; }

        public IEnumerable<KeyValuePair<string, Argument>> Arguments
        {
            get => _arguments;
            set
            {
                if (value == null)
                {
                    _arguments.Clear();
                    return;
                }

                foreach (var argument in value)
                    _arguments[argument.Key] = argument.Value;
            }
        }

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