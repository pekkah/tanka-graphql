using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
            object rawValue = default;

            if (_arguments.ContainsKey(name))
            {
                rawValue = _arguments[name];           
            }
            else
            {

                var argument = Type.GetArgument(name);
                rawValue = argument?.DefaultValue;
            }

            if (rawValue == null || rawValue.Equals(default(T)))
                return default(T);

            if (rawValue is T argAsType)
                return argAsType;

            //todo(pekka): should not depend directly on JSON.Net
            var obj = JObject.FromObject(rawValue);

            return obj.ToObject<T>();
        }

        public override string ToString()
        {
            return $"@{Name}";
        }
    }
}