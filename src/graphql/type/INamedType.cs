namespace tanka.graphql.type
{
    public interface INamedType: IType
    {
        string Name { get; }
    }
}