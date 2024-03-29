﻿using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Fields;

public class FieldException : Exception
{
    public FieldException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }

    public required FieldDefinition? Field { get; set; }

    public required FieldSelection Selection { get; set; }

    public required NodePath Path { get; set; }

    public required ObjectDefinition ObjectDefinition { get; set; }
}