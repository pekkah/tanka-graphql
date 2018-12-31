namespace tanka.graphql.type
{
    public static class GraphQLTypeExtensions 
    {
        public static IGraphQLType Unwrap(this IGraphQLType type)
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