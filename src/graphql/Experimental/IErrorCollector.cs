using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental;

public interface IErrorCollector
{
    static IErrorCollector Default() => new ConcurrentBagErrorCollector();
    
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