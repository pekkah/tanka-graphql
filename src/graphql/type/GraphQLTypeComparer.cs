using System;
using System.Collections.Generic;

namespace fugu.graphql.type
{
    public class GraphQLTypeComparer : IEqualityComparer<IGraphQLType>
    {
        public bool Equals(IGraphQLType x, IGraphQLType y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal);
        }

        public int GetHashCode(IGraphQLType obj)
        {
            return obj.Name?.GetHashCode() ?? string.Empty.GetHashCode();
        }
    }
}