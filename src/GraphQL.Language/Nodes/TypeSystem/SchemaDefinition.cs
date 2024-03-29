﻿using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class SchemaDefinition : INode
{
    public SchemaDefinition(
        StringValue? description,
        Directives? directives,
        RootOperationTypeDefinitions operations,
        in Location? location = default)
    {
        Description = description;
        Directives = directives;
        Operations = operations;
        Location = location;
    }

    public StringValue? Description { get; }

    public Directives? Directives { get; }

    public RootOperationTypeDefinitions Operations { get; }
    public NodeKind Kind => NodeKind.SchemaDefinition;

    public Location? Location { get; }

    public static implicit operator SchemaDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseSchemaDefinition();
    }

    public static implicit operator SchemaDefinition(ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseSchemaDefinition();
    }

    public override string ToString()
    {
        return Printer.Print(this);
    }
}