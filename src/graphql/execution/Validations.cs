using System.Collections.Generic;
using fugu.graphql.type;

namespace fugu.graphql.execution
{
    public static class Validations
    {
        public static bool IsInputType(IGraphQLType type)
        {
            if (type is IWrappingType wrappingType)
            {
                return IsInputType(wrappingType.WrappedType);
            }

            if (type is ScalarType)
            {
                return true;
            }

            if (type is EnumType)
            {
                return true;
            }

            if (type is InputObjectType)
            {
                return true;
            }

            return false;
        }
    }
}