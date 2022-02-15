using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Visitors;

namespace Tanka.GraphQL.Language.Validation;

public class OperationNameUniquenessRule :
    IVisit<OperationDefinition>,
    IVisit<ExecutableDocument>
{
    private readonly Stack<Name> _known = new(2);

    public void Enter(ExecutableDocument node)
    {
    }

    public void Leave(ExecutableDocument node)
    {
        while (_known.TryPop(out var name))
            if (_known.Contains(name))
                throw new InvalidOperationException();
        /*context.Error(ValidationErrorCodes.R5211OperationNameUniqueness,
                "Each named operation definition must be unique within a " +
                "document when referred to by its name. " +
                $"Operation: '{operationName}'",
                definition);*/
    }

    public void Enter(OperationDefinition definition)
    {
        if (definition.Name == null)
            return;

        var operationName = definition.Name.Value;
        _known.Push(operationName);
    }

    public void Leave(OperationDefinition node)
    {
    }
}