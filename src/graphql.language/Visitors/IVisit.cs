namespace Tanka.GraphQL.Language.Visitors
{
    public interface IVisit<in T>
    {
        void Enter(T node);
        void Leave(T node);
    }
}