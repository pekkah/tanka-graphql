using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
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
            object rawValue;

            if (_arguments.ContainsKey(name))
            {
                rawValue = _arguments[name];           
            }
            else
            {

                var argument = Type.GetArgument(name);
                rawValue = argument?.DefaultValue;
            }

            if (rawValue == null || rawValue.Equals(null))
                return default(T);

            return (T)rawValue;
        }

        public override string ToString()
        {
            return $"@{Name}";
        }
    }
}