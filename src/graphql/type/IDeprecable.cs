namespace tanka.graphql.type
{
    public interface IDeprecable
    {
        string DeprecationReason { get; }

        bool IsDeprecated { get; }
    }
}