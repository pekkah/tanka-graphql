namespace tanka.graphql.type
{
    public interface IWrappingType
    {
        IGraphQLType WrappedType { get; }
    }
}