using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class Args : Dictionary<string, Argument>
    {
        public Args()
        {
            
        }

        public void Add(string key, IType type, object defaultValue = null, Meta meta = null)
        {
            Add(key, new Argument(type, defaultValue, meta));
        }

        public Args(IEnumerable<KeyValuePair<string, Argument>> arguments)
        {
            foreach (var argument in arguments)
            {
                this[argument.Key] = argument.Value;
            }
        }
    }
}