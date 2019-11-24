using System.Collections.Generic;
using System.Linq;

namespace Tanka.GraphQL.Execution
{
    public class NodePath
    {
        private readonly List<object> _path = new List<object>();

        public NodePath()
        {
        }

        protected NodePath(object[] segments)
        {
            _path.AddRange(segments);
        }

        public IEnumerable<object> Segments => _path;

        public NodePath Append(string fieldName)
        {
            _path.Add(fieldName);
            return this;
        }

        public NodePath Append(int index)
        {
            _path.Add(index);
            return this;
        }

        public NodePath Fork()
        {
            return new NodePath(Segments.ToArray());
        }
    }
}