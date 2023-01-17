using System.Collections.Concurrent;

namespace Tanka.GraphQL;

public interface IErrorCollector
{
    static IErrorCollector Default()
    {
        return new ConcurrentBagErrorCollector();
    }

    public void Add(Exception error);

    public IEnumerable<Exception> GetErrors();
}

public class ConcurrentBagErrorCollector : IErrorCollector
{
    private readonly ConcurrentBag<Exception> _bag = new();

    public void Add(Exception error)
    {
        _bag.Add(error);
    }

    public IEnumerable<Exception> GetErrors()
    {
        return _bag;
    }
}