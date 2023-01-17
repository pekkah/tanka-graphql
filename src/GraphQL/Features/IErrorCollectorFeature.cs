namespace Tanka.GraphQL.Features;

public interface IErrorCollectorFeature
{
    public IErrorCollector ErrorCollector { get; set; }
}