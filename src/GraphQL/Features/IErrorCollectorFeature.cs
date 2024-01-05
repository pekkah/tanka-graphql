namespace Tanka.GraphQL.Features;

public interface IErrorCollectorFeature
{
    void Add(Exception error);
    IEnumerable<ExecutionError> GetErrors();
    ExecutionError FormatError(Exception x);
}