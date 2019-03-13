using System;
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class DirectiveInstance
    {
        public DirectiveType Type { get; }

        private readonly Dictionary<string, object> _arguments;

        public DirectiveInstance(DirectiveType directiveType, Dictionary<string, object> argumentValues = null)
        {
            Type = directiveType ?? throw new ArgumentNullException(nameof(directiveType));
            _arguments = argumentValues ?? new Dictionary<string, object>();
        }

        public string Name => Type.Name;

        public T GetArgument<T>(string name)
        {
            if (_arguments.ContainsKey(name))
            {
                return (T) _arguments[name];
            }

            var argument = Type.GetArgument(name);
            return (T)argument?.DefaultValue;
        }

        public override string ToString()
        {
            return $"@{Name}";
        }
    }
}