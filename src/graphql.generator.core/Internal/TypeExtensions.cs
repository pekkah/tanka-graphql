using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    internal static class TypeExtensions
    {
        public static bool IsList(this IType type)
        {
            if (type is NonNull nonNull)
            {
                return IsList(nonNull.OfType);
            }

            if (type is List)
                return true;

            return false;
        }
    }
}