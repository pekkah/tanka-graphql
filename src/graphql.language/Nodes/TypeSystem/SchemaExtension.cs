using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class SchemaExtension: INode
    {
        public NodeKind Kind => NodeKind.SchemaExtension;
        public SchemaExtension(
            StringValue? description,
            Directives? directives,
            RootOperationTypeDefinitions? operations,
            in Location? location = default)
        {
            Description = description;
            Directives = directives;
            Operations = operations;
            Location = location;
        }

        public StringValue? Description { get; }
        public Directives? Directives { get; }
        public RootOperationTypeDefinitions? Operations { get; }
        public Location? Location { get; }

        public static implicit operator SchemaExtension(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseSchemaExtension(hasExtend:true);
        }

        public static implicit operator SchemaExtension(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseSchemaExtension(hasExtend: true);
        }
    }
}