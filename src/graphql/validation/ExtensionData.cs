using System.Collections.Generic;

namespace tanka.graphql.validation
{
    public class ExtensionData
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public void Set(string key, object data)
        {
            _data[key] = data;
        }

        public IReadOnlyDictionary<string, object> Data => _data;
    }
}