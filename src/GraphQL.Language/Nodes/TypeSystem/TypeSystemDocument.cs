﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class TypeSystemDocument : INode
{
    public TypeSystemDocument(
        IReadOnlyList<SchemaDefinition>? schemaDefinitions,
        IReadOnlyList<TypeDefinition>? typeDefinitions,
        IReadOnlyList<DirectiveDefinition>? directiveDefinitions,
        IReadOnlyList<SchemaExtension>? schemaExtensions = default,
        IReadOnlyList<TypeExtension>? typeExtensions = default,
        IReadOnlyList<Import>? imports = default)
    {
        SchemaDefinitions = schemaDefinitions;
        TypeDefinitions = typeDefinitions;
        DirectiveDefinitions = directiveDefinitions;
        SchemaExtensions = schemaExtensions;
        TypeExtensions = typeExtensions;
        Imports = imports;
    }

    public IReadOnlyList<DirectiveDefinition>? DirectiveDefinitions { get; }

    public IReadOnlyList<Import>? Imports { get; }

    public IReadOnlyList<SchemaDefinition>? SchemaDefinitions { get; }

    public IReadOnlyList<SchemaExtension>? SchemaExtensions { get; }

    public IReadOnlyList<TypeDefinition>? TypeDefinitions { get; }

    public IReadOnlyList<TypeExtension>? TypeExtensions { get; }

    public NodeKind Kind => NodeKind.TypeSystemDocument;

    public Location? Location => null;

    public static implicit operator TypeSystemDocument(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseTypeSystemDocument();
    }

    public static implicit operator TypeSystemDocument(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseTypeSystemDocument();
    }
}