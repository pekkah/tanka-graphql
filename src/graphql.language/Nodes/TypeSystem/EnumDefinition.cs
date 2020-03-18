using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class EnumDefinition
    {
        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public IReadOnlyCollection<EnumValueDefinition>? Values { get; }
        public Location? Location { get; }

        public EnumDefinition(
            StringValue? description, 
            Name name, 
            IReadOnlyCollection<Directive>? directives, 
            IReadOnlyCollection<EnumValueDefinition>? values,
            in Location? location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Values = values;
            Location = location;
        }
    }
}