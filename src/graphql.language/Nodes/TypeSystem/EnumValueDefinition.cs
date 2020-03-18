using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class EnumValueDefinition
    {
        public StringValue? Description { get; }
        public EnumValue Value { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location? Location { get; }

        public EnumValueDefinition(
            StringValue? description,
            EnumValue value, 
            IReadOnlyCollection<Directive>? directives, 
            in Location? location)
        {
            Description = description;
            Value = value;
            Directives = directives;
            Location = location;
        }
    }
}