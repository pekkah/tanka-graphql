using System.Collections.Generic;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public class Field : IField, IDescribable, IDeprecable, IHasDirectives
    {
        private readonly Args _arguments = new Args();
        private readonly DirectiveList _directives;

        public Field(
            IType type,
            Args arguments = null,
            string description = null,
            object defaultValue = null,
            IEnumerable<DirectiveInstance> directives = null,
            string deprecationReason = null)
        {
            Type = type;
            Description = description ?? string.Empty;
            DefaultValue = defaultValue;
            DeprecationReason = deprecationReason;
            _directives = new DirectiveList(directives);

            if (arguments != null)
                foreach (var argument in arguments)
                    _arguments[argument.Key] = argument.Value;
        }

        public object DefaultValue { get; set; }


        public Resolver Resolve { get; set; }

        public Subscriber Subscribe { get; set; }

        public string DeprecationReason { get; }

        public bool IsDeprecated => !string.IsNullOrEmpty(DeprecationReason);

        public string Description { get; }


        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }

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