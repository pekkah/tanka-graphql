using System.Collections.Generic;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public static class TypeIs
    {
        public static bool IsInputType(IType type)
        {
            if (type is IWrappingType wrappingType)
            {
                return IsInputType(wrappingType.OfType);
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

        public static bool IsOutputType(IType type)
        {
            if (type is IWrappingType wrappingType)
            {
                return IsOutputType(wrappingType.OfType);
            }

            if (type is ScalarType)
            {
                return true;
            }

            if (type is ObjectType)
            {
                return true;
            }

            if (type is InterfaceType)
            {
                return true;
            }

            if (type is UnionType)
            {
                return true;
            }

            if (type is EnumType)
            {
                return true;
            }

            return false;
        }
    }
}