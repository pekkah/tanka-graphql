using System;
using System.Collections.Generic;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueResolution;

public class CompleteValueException : QueryExecutionException
{
    public CompleteValueException(string message, NodePath path, params INode[] nodes) : base(message, path,
        nodes)
    {
    }

    public CompleteValueException(string message, Exception innerException, NodePath path, params INode[] nodes) :
        base(message, innerException, path, nodes)
    {
    }

    public CompleteValueException(string message, Exception innerException, NodePath path,
        IReadOnlyDictionary<string, object> extensions, params INode[] nodes) : base(message, innerException,
        path, extensions, nodes)
    {
    }
}