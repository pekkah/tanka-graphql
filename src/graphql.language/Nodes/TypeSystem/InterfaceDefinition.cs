using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InterfaceDefinition : TypeDefinition
    {
        public override NodeKind Kind => NodeKind.InterfaceDefinition;

        public InterfaceDefinition(
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

        public StringValue? Description { get; }
        public override Name Name { get; }
        public ImplementsInterfaces? Interfaces { get; }
        public Directives? Directives { get; }
        public FieldsDefinition? Fields { get; }
        public override Location? Location { get; }

        public static implicit operator InterfaceDefinition(string value)
        {
            var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
            return parser.ParseInterfaceDefinition();
        }

        public static implicit operator InterfaceDefinition(in ReadOnlySpan<byte> value)
        {
            var parser = Parser.Create(value);
            return parser.ParseInterfaceDefinition();
        }
    }
}