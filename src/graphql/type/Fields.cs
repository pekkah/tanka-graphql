using System.Collections.Generic;

namespace tanka.graphql.type
{
    public sealed class Fields : Dictionary<string, IField>
    {
        public void Add(string key, IType type, Args arguments = null, Meta meta = null, object defaultValue = null)
        {
            Add(key, new Field(type, arguments, meta, defaultValue));
        }

        public Fields(IEnumerable<KeyValuePair<string, IField>> fields)
        {
            foreach (var kv in fields)
            {
                Add(kv.Key, kv.Value);
            }
        }

        public Fields()
        {
            
        }
    }
}