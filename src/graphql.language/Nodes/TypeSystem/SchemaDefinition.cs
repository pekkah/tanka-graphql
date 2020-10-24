using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class SchemaDefinition: INode
    {
        public NodeKind Kind => NodeKind.SchemaDefinition;

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

        public Location? Location { get; }

        public static implicit operator SchemaDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseSchemaDefinition();
        }

        public static implicit operator SchemaDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseSchemaDefinition();
        }
    }
}