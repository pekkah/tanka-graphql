namespace tanka.graphql.type
{
    public static class TypeExtensions 
    {
        public static INamedType Unwrap(this IType type)
        {
            switch (type)
            {
                case NonNull nonNull:
                    return Unwrap(nonNull.WrappedType);
                case List list:
                    return Unwrap(list.WrappedType);
            }

            return (INamedType)type;
        }
    }
}