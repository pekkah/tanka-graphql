namespace Tanka.GraphQL.TypeSystem
{
    public interface IWrappingType: IType
    {
        IType OfType { get; }
    }
}