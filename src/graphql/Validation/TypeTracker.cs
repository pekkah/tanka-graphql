using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation;

public class TypeTracker : RuleVisitor
{
    public Stack<NamedType?> ParentTypes { get; } = new();

    public TypeTracker(ISchema schema)
    {

    }

    public NamedType? ParentType => ParentTypes.Count > 0 ? ParentTypes.Peek() : null;
}