using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ExecutableDocument: INode
    {
        public NodeKind Kind => NodeKind.ExecutableDocument;
        public Location? Location => null;
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;
        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public ExecutableDocument(
            IReadOnlyCollection<OperationDefinition>? operationDefinitions,
            IReadOnlyCollection<FragmentDefinition>? fragmentDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = fragmentDefinitions;
        }

        public static implicit operator ExecutableDocument(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseExecutableDocument();
        }

        public static implicit operator ExecutableDocument(ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseExecutableDocument();
        }
    }
}