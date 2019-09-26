using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public class Args : Dictionary<string, Argument>
    {
        public Args()
        {
            
        }

        public void Add(string key, IType type, object defaultValue = null, string description = null)
        {
            Add(key, new Argument(type, defaultValue, description));
        }

        public Args(IEnumerable<KeyValuePair<string, Argument>> arguments)
        {
            foreach (var argument in arguments)
            {
                this[argument.Key] = argument.Value;
            }
        }

        public Args((string Name, IType Type, object DefaultValue, string Description)[] args)
        {
            foreach (var (name, type, defaultValue, description) in args)
            {
                Add(name, type, defaultValue, description);
            }
        }
    }
}