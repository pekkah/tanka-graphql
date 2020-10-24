using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class UnionDefinitionExtensions
    {
        public static UnionDefinition WithDescription(this UnionDefinition definition, 
            in StringValue? description)
        {
            return new UnionDefinition(
                description,
                definition.Name,
                definition.Directives,
                definition.Members,
                definition.Location);
        }

        public static UnionDefinition WithName(this UnionDefinition definition, 
            in Name name)
        {
            return new UnionDefinition(
                definition.Description,
                name,
                definition.Directives,
                definition.Members,
                definition.Location);
        }
        public static UnionDefinition WithDirectives(this UnionDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new UnionDefinition(
                definition.Description,
                definition.Name,
                Directives.From(directives), 
                definition.Members,
                definition.Location);
        }

        public static UnionDefinition WithMembers(this UnionDefinition definition,
            IReadOnlyList<NamedType>? members)
        {
            return new UnionDefinition(
                definition.Description,
                definition.Name,
                definition.Directives,
                UnionMemberTypes.From(members),
                definition.Location);
        }
    }
}