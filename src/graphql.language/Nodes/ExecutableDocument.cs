using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ExecutableDocument
    {
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
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseExecutableDocument();
        }

        public static implicit operator string(ExecutableDocument value)
        {
            throw new NotImplementedException();
        }
    }
}