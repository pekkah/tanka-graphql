namespace Tanka.GraphQL.Features;

public class ErrorCollectorFeature : IErrorCollectorFeature
{
    public required IErrorCollector ErrorCollector { get; set; }
}