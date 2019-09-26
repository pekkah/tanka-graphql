namespace Tanka.GraphQL.TypeSystem
{
    public static class TypeExtensions 
    {
        public static INamedType Unwrap(this IType type)
        {
            switch (type)
            {
                case NonNull nonNull:
                    return Unwrap(nonNull.OfType);
                case List list:
                    return Unwrap(list.OfType);
            }

            return (INamedType)type;
        }
    }
}