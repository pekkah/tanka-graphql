using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class UnionDefinition
    {
        public StringValue? Description { get; }
        
        public Name Name { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }
        
        public IReadOnlyCollection<NamedType>? Members { get; }
        
        public Location? Location { get; }

        public UnionDefinition(
            StringValue? description, 
            Name name, 
            IReadOnlyCollection<Directive>? directives, 
            IReadOnlyCollection<NamedType>? members, 
            in Location? location)
        {
            Description = description;
            Name = name;
            Directives = directives;
            Members = members;
            Location = location;
        }
    }
}