namespace fugu.graphql.type
{
    public interface IWrappingType
    {
        IGraphQLType WrappedType { get; }
    }
}