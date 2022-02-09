using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class InterfaceDefinitionExtensions
    {
        public static bool TryGetDirective(
            this InterfaceDefinition definition,
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

        public static bool TryImplements(
            this InterfaceDefinition definition,
            Name interfaceName,
            [NotNullWhen(true)] out NamedType? namedType)
        {
            if (definition.Interfaces is null)
            {
                namedType = null;
                return false;
            }

            return definition.Interfaces.TryGet(interfaceName, out namedType);
        }

        public static bool Implements(
            this InterfaceDefinition definition,
            Name interfaceName)
        {
            return definition.Interfaces?.Any(i => i.Name == interfaceName) == true;
        }

        public static InterfaceDefinition WithDescription(this InterfaceDefinition definition,
            in StringValue? description)
        {
            return new InterfaceDefinition(
                description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithName(this InterfaceDefinition definition,
            in Name name)
        {
            return new InterfaceDefinition(
                definition.Description,
                name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithInterfaces(this InterfaceDefinition definition,
            IReadOnlyList<NamedType>? interfaces)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                ImplementsInterfaces.From(interfaces),
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithDirectives(this InterfaceDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                Directives.From(directives),
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithFields(this InterfaceDefinition definition,
            IReadOnlyList<FieldDefinition>? fields)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                FieldsDefinition.From(fields),
                definition.Location);
        }
    }
}