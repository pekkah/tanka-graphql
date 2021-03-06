﻿using System.Collections.Generic;

namespace Tanka.GraphQL.TypeSystem
{
    public sealed class Fields : Dictionary<string, IField>
    {
        public void Add(string key, IType type, Args arguments = null, string description = null, object defaultValue = null)
        {
            Add(key, new Field(type, arguments, description, defaultValue));
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