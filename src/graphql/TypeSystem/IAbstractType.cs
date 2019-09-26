namespace Tanka.GraphQL.TypeSystem
{
    public interface IAbstractType
    {
        bool IsPossible(ObjectType type);
    }
}