using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ObjectDefinition : TypeDefinition
    {
        public override NodeKind Kind => NodeKind.ObjectDefinition;
        public ObjectDefinition(
            StringValue? description,
            in Name name,
            IReadOnlyCollection<NamedType>? interfaces,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<FieldDefinition>? fields,
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
        public IReadOnlyCollection<NamedType>? Interfaces { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<FieldDefinition>? Fields { get; }
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