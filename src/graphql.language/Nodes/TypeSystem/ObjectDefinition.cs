using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ObjectDefinition : TypeDefinition
    {
        public ObjectDefinition(
            StringValue? description,
            in Name name,
            ImplementsInterfaces? interfaces,
            Directives? directives,
            FieldsDefinition? fields,
            in Location? location = default)
        {
            Description = description;
            Name = name;
            Interfaces = interfaces;
            Directives = directives;
            Fields = fields;
            Location = location;
        }

        public override NodeKind Kind => NodeKind.ObjectDefinition;

        public StringValue? Description { get; }
        public override Name Name { get; }
        public ImplementsInterfaces? Interfaces { get; }
        public Directives? Directives { get; }
        public FieldsDefinition? Fields { get; }
        public override Location? Location { get; }

        public static implicit operator ObjectDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseObjectDefinition();
        }

        public static implicit operator ObjectDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseObjectDefinition();
        }
    }
}