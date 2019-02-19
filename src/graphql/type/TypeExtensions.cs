namespace tanka.graphql.type
{
    public static class TypeExtensions 
    {
        public static IType Unwrap(this IType type)
        {
            switch (type)
            {
                case NonNull nonNull:
                    return Unwrap(nonNull.WrappedType);
                case List list:
                    return Unwrap(list.WrappedType);
            }

            return type;
        }
    }
}