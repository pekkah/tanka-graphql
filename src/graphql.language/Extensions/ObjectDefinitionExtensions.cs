using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class ObjectDefinitionExtensions
    {
        public static bool TryImplements(
            this ObjectDefinition definition,
            Name interfaceName,
            [NotNullWhen(true)]out NamedType? namedType)
        {
            if (definition.Interfaces is null)
            {
                namedType = null;
                return false;
            }

            return definition.Interfaces.TryGet(interfaceName, out namedType);
        }

        public static bool HasInterface(
            this ObjectDefinition definition,
            Name interfaceName)
        {
            return definition.Interfaces?.Any(i => i.Name == interfaceName) == true;
        }

        public static bool TryGetDirective(
            this ObjectDefinition definition,
            Name directiveName,
            [NotNullWhen(true)]out Directive? directive)
        {
            if (definition.Directives is null)
            {
                directive = null;
                return false;
            }

            return definition.Directives.TryGet(directiveName, out directive);
        }

        public static ObjectDefinition WithDescription(this ObjectDefinition definition, 
            in StringValue? description)
        {
            return new ObjectDefinition(
                description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithName(this ObjectDefinition definition, 
            in Name name)
        {
            return new ObjectDefinition(
                definition.Description,
                name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithInterfaces(this ObjectDefinition definition,
            IReadOnlyList<NamedType>? interfaces)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                ImplementsInterfaces.From(interfaces), 
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithDirectives(this ObjectDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                Directives.From(directives), 
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithFields(this ObjectDefinition definition,
            IReadOnlyList<FieldDefinition>? fields)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                FieldsDefinition.From(fields),
                definition.Location);
        }
    }
}