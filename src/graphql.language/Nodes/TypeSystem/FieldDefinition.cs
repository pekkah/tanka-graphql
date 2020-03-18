using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class FieldDefinition
    {
        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<InputValueDefinition>? ArgumentDefinitions { get; }
        public IType Type { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public FieldDefinition(StringValue? description,
            Name name,
            IReadOnlyCollection<InputValueDefinition>? argumentDefinitions,
            IType type,
            IReadOnlyCollection<Directive>? directives,
            in Location? location)
        {
            Description = description;
            Name = name;
            ArgumentDefinitions = argumentDefinitions;
            Type = type;
            Directives = directives;
            Location = location;
        }
    }
}