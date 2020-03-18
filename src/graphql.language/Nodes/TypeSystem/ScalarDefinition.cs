using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class ScalarDefinition
    {
        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<Directive>? Directives { get; }
        public Location Location { get; }

        public ScalarDefinition(
            StringValue? description, 
            Name name, 
            IReadOnlyCollection<Directive>? directives, 
            in Location location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Location = location;
        }
    }
}