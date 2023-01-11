namespace Tanka.GraphQL.Experimental.Features;

public interface IErrorCollectorFeature
{
    public IErrorCollector ErrorCollector { get; set; }
}