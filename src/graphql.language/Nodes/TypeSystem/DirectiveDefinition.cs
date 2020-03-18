using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class DirectiveDefinition
    {
        public StringValue? Description { get; }
        public Name Name { get; }
        public IReadOnlyCollection<InputValueDefinition>? ArgumentDefinitions { get; }
        public bool IsRepeatable { get; }
        public IReadOnlyCollection<string> DirectiveLocations { get; }
        public Location Location { get; }

        public DirectiveDefinition(
            StringValue? description, 
            Name name, 
            IReadOnlyCollection<InputValueDefinition>? argumentDefinitions, 
            in bool isRepeatable, 
            IReadOnlyCollection<string> directiveLocations, 
            in Location location)
        {
            Description = description;
            Name = name;
            ArgumentDefinitions = argumentDefinitions;
            IsRepeatable = isRepeatable;
            DirectiveLocations = directiveLocations;
            Location = location;
        }
    }
}