using System.Collections.Generic;

namespace fugu.graphql
{
    public class NodePath
    {
        public static implicit operator List<object>(NodePath path)
        {
            if (path == null)
                return null;

            return new List<object>();
        }
    }
}