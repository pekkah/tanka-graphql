using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class InputValueDefinition
    {
        public StringValue? Description { get; }
        
        public Name Name { get; }
        
        public IType Type { get; }
        
        public DefaultValue? DefaultValue { get; }

        public IReadOnlyCollection<Directive>? Directives { get; }

        public Location Location { get; }

        public InputValueDefinition(
            StringValue? description, 
            Name name, 
            IType type, 
            DefaultValue? defaultValue, 
            IReadOnlyCollection<Directive>? directives, 
            in Location location)
        {
            Description = description;
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Directives = directives;
            Location = location;
        }
    }
}