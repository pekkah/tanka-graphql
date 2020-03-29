using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ObjectDefinition : TypeDefinition
    {
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
        public Name Name { get; }
        public IReadOnlyCollection<NamedType>? Interfaces { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<FieldDefinition>? Fields { get; }
        public Location? Location { get; }

        public static implicit operator ObjectDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseObjectDefinition();
        }

        public static implicit operator string(ObjectDefinition value)
        {
            throw new NotImplementedException();
        }
    }
}