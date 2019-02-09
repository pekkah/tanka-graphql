using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public static class Wrapper
    {
        public static IType WrapIfRequired(IType templateType, IType maybeRequiresWrapping)
        {
            if (templateType is NonNull nonNull)
                return new NonNull(WrapIfRequired(nonNull.WrappedType, maybeRequiresWrapping));

            if (templateType is List list)
                return new List(WrapIfRequired(list.WrappedType, maybeRequiresWrapping));

            return maybeRequiresWrapping;
        }
    }
}