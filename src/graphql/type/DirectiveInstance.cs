using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class DirectiveInstance
    {
        public DirectiveType Type { get; }

        private readonly Dictionary<string, Argument> _arguments = new Dictionary<string, Argument>();

        public DirectiveInstance(DirectiveType directiveType, Args arguments = null)
        {
            Type = directiveType;

            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    _arguments[argument.Key] = argument.Value;
                }
            }
        }

        public string Name => Type.Name;

        public Argument GetArgument(string name)
        {
            return _arguments.ContainsKey(name) ? _arguments[name] : null;
        }
    }
}