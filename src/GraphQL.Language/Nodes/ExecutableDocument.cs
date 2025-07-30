using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class ExecutableDocument : INode
{
    public readonly FragmentDefinitions? FragmentDefinitions;
    public readonly OperationDefinitions? OperationDefinitions;

    public ExecutableDocument(
        OperationDefinitions? operationDefinitions,
        FragmentDefinitions? fragmentDefinitions)
    {
        OperationDefinitions = operationDefinitions;
        FragmentDefinitions = fragmentDefinitions;
    }

    public NodeKind Kind => NodeKind.ExecutableDocument;
    public Location? Location => null;

    public static implicit operator ExecutableDocument(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseExecutableDocument();
    }

    public static implicit operator ExecutableDocument(ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseExecutableDocument();
    }

    public override string ToString()
    {
        return Printer.Print(this);
    }
}