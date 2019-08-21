using System;
using System.Collections.Generic;

namespace tanka.graphql.type
{
    public class GraphQLTypeComparer : IEqualityComparer<IType>
    {
        public bool Equals(IType x, IType y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));

            if (x is List listX && y is List listY)
            {
                return Object.Equals(listX.OfType, listY.OfType);
            }

            if (x is NonNull nonNullX && y is NonNull nonNullY)
            {
                return Object.Equals(nonNullX.OfType, nonNullY.OfType);
            }

            if (x is INamedType namedTypeX && y is INamedType namedTypeY)
            {
                return Object.Equals(namedTypeX, namedTypeY);
            }

            return false;
        }

        public int GetHashCode(IType obj)
        {
            return obj.GetHashCode();
        }
    }
}