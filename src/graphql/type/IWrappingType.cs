namespace tanka.graphql.type
{
    public interface IWrappingType: IType
    {
        IType WrappedType { get; }
    }
}