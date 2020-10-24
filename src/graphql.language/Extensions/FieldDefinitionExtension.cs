using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class FieldDefinitionExtension
    {
        public static FieldDefinition WithDescription(this FieldDefinition definition, in StringValue description)
        {
            return new FieldDefinition(
                description,
                definition.Name,
                definition.Arguments,
                definition.Type,
                definition.Directives,
                definition.Location);
        }

        public static FieldDefinition WithName(this FieldDefinition definition, in Name name)
        {
            return new FieldDefinition(
                definition.Description,
                name,
                definition.Arguments,
                definition.Type,
                definition.Directives,
                definition.Location);
        }

        public static FieldDefinition WithArguments(this FieldDefinition definition,
            IReadOnlyList<InputValueDefinition>? arguments)
        {
            return new FieldDefinition(
                definition.Description,
                definition.Name,
                ArgumentsDefinition.From(arguments), 
                definition.Type,
                definition.Directives,
                definition.Location);
        }

        public static FieldDefinition WithType(this FieldDefinition definition, TypeBase type)
        {
            return new FieldDefinition(
                definition.Description,
                definition.Name,
                definition.Arguments,
                type,
                definition.Directives,
                definition.Location);
        }

        public static FieldDefinition WithDirectives(this FieldDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new FieldDefinition(
                definition.Description,
                definition.Name,
                definition.Arguments,
                definition.Type,
                Directives.From(directives),
                definition.Location);
        }
    }
}