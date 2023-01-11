namespace Tanka.GraphQL.Experimental.Features;

public class ErrorCollectorFeature : IErrorCollectorFeature
{
    public required IErrorCollector ErrorCollector { get; set; }
}