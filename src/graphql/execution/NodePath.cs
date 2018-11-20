using System.Collections.Generic;
using System.Linq;

namespace fugu.graphql.execution
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

        public void Append(string fieldName)
        {
            _path.Add(fieldName);
        }

        public void Append(int index)
        {
            _path.Add(index);
        }

        public NodePath Fork()
        {
            return new NodePath(Segments.ToArray());
        }
    }
}