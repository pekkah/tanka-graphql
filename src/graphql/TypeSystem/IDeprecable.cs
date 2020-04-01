namespace Tanka.GraphQL.TypeSystem
{
    public interface IDeprecable
    {
        string? DeprecationReason { get; }

        bool IsDeprecated { get; }
    }
}