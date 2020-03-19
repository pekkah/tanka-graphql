using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputObjectDefinition : ITypeDefinition
    {
        public InputObjectDefinition(
            StringValue? description,
            Name name,
            IReadOnlyCollection<Directive>? directives,
            IReadOnlyCollection<InputValueDefinition>? fields,
            in Location? location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Fields = fields;
            Location = location;
        }

        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<InputValueDefinition>? Fields { get; }
        public Location? Location { get; }
    }
}