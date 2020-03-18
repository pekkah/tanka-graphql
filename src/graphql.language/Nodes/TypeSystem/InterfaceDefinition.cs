using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InterfaceDefinition
    {
        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<NamedType>? Interfaces { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<FieldDefinition>? Fields { get; }
        public Location? Location { get; }

        public InterfaceDefinition(
            StringValue? description, 
            Name name, 
            IReadOnlyCollection<NamedType>? interfaces, 
            IReadOnlyCollection<Directive>? directives, 
            IReadOnlyCollection<FieldDefinition>? fields, 
            in Location? location)
        {
            Description = description;
            Name = name;
            Interfaces = interfaces;
            Directives = directives;
            Fields = fields;
            Location = location;
        }
    }
}