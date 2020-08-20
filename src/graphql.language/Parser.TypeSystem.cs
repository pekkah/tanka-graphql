using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public ref partial struct Parser
    {
        public TypeSystemDocument ParseTypeSystemDocument()
        {
            var schemaDefinitions = new List<SchemaDefinition>();
            var typeDefinitions = new List<TypeDefinition>();
            var directiveDefinitions = new List<DirectiveDefinition>();
            var schemaExtensions = new List<SchemaExtension>();
            var typeExtensions = new List<TypeExtension>();
            var imports = new List<Import>();

            // check for tanka imports
            if (_lexer.Kind == TokenKind.BlockStringValue)
            {
                if (TryParseTankaImports(out var foundImports))
                {
                    imports.AddRange(foundImports ?? Enumerable.Empty<Import>());
                }
            }

            while (_lexer.Kind != TokenKind.End)
            {
                switch (_lexer.Kind)
                {
                    case TokenKind.StringValue:
                    case TokenKind.BlockStringValue:
                        // this will reserve the description
                        PreParseOptionalDescription();
                        continue;
                    case TokenKind.Name:
                        // type, scalar etc.
                        if (Keywords.IsTypeDefinition(_lexer.Value))
                        {
                            typeDefinitions.Add(ParseTypeDefinition());
                            continue;
                        }
                        // schema
                        else if (Keywords.Schema.Match(_lexer.Value))
                        {
                            schemaDefinitions.Add(ParseSchemaDefinition());
                            continue;
                        }
                        // directive
                        else if (Keywords.Directive.Match(_lexer.Value))
                        {
                            directiveDefinitions.Add(ParseDirectiveDefinition());
                            continue;
                        }
                        // extend
                        else if (Keywords.Extend.Match(_lexer.Value))
                        {
                            _lexer.Advance();

                            // types
                            if (Keywords.IsTypeDefinition(_lexer.Value))
                            {
                                typeExtensions.Add(ParseTypeExtension(hasExtend: false));
                                continue;
                            }
                            else if (Keywords.Schema.Match(_lexer.Value))
                            {
                                schemaExtensions.Add(ParseSchemaExtension(hasExtend: false));
                                continue;
                            }
                        }
                        else if (Keywords.Import.Match(_lexer.Value))
                        {
                            imports.Add(ParseTankaImport());
                            continue;
                        }
                        break;
                        
                }

                throw new Exception($"Unexpected token {_lexer.Kind} at {_lexer.Line}:{_lexer.Column}");
            }

            return new TypeSystemDocument(
                schemaDefinitions,
                typeDefinitions,
                directiveDefinitions,
                schemaExtensions,
                typeExtensions,
                imports);
        }

        public Import ParseTankaImport()
        {
            /* """
             * tanka_import Types[]? from From
             * """
             */

            /* From: StringValue */

            /* ex. tanka_import from "./types/person" */
            /* ex. tanka_import Person from "./types/person" */

            Ensure(TokenKind.BlockStringValue);
            var blockStringValue = _lexer.Value;

            var importParser = Parser.Create(blockStringValue);
            var import = importParser.ParseTankaImportInternal();
            Skip(TokenKind.BlockStringValue);
            
            return import;
        }

        public bool TryParseTankaImports(out IReadOnlyList<Import>? imports)
        {
            if (_lexer.Kind != TokenKind.BlockStringValue)
            {
                imports = null;
                return false;
            }

            var blockStringValue = _lexer.Value;

            var importParser = Parser.Create(blockStringValue);

            if (!Keywords.Import.Match(importParser._lexer.Value))
            {
                imports = null;
                return false;
            }

            var _imports = new List<Import>();
            while (Keywords.Import.Match(importParser._lexer.Value))
            {
                var import = importParser.ParseTankaImportInternal();
                _imports.Add(import);
            }

            Skip(TokenKind.BlockStringValue);
            imports = _imports;
            return true;
        }

        private Import ParseTankaImportInternal()
        {
            var location = SkipKeyword(Keywords.Import.Span);

            var types = new List<Name>();
            if (!Keywords.From.Match(_lexer.Value))
            {
                // types
                while (!Keywords.From.Match(_lexer.Value) && _lexer.Kind == TokenKind.Name)
                {
                    types.Add(ParseName());
                }
            }

            // from
            SkipKeyword(Keywords.From.Span);

            // from
            var from = ParseStringValue();

            return new Import(
                types.Any() ? types : null,
                from,
                location);
        }

        public TypeDefinition ParseTypeDefinition()
        {
            if (Keywords.Scalar.Match(_lexer.Value))
                return ParseScalarDefinition();

            if (Keywords.Type.Match(_lexer.Value))
                return ParseObjectDefinition();

            if (Keywords.Interface.Match(_lexer.Value))
                return ParseInterfaceDefinition();

            if (Keywords.Union.Match(_lexer.Value))
                return ParseUnionDefinition();

            if (Keywords.Enum.Match(_lexer.Value))
                return ParseEnumDefinition();

            if (Keywords.Input.Match(_lexer.Value))
                return ParseInputObjectDefinition();

            throw new Exception(
                $"Unexpected type definition :'{Encoding.UTF8.GetString(_lexer.Value)}'.");
        }

        public TypeExtension ParseTypeExtension(bool hasExtend = true)
        {
            if (Keywords.Scalar.Match(_lexer.Value))
                return ParseScalarExtension(hasExtend);

            if (Keywords.Type.Match(_lexer.Value))
                return ParseObjectExtension(hasExtend);

            if (Keywords.Interface.Match(_lexer.Value))
                return ParseInterfaceExtension(hasExtend);

            if (Keywords.Union.Match(_lexer.Value))
                return ParseUnionExtension(hasExtend);

            if (Keywords.Enum.Match(_lexer.Value))
                return ParseEnumExtension(hasExtend);

            if (Keywords.Input.Match(_lexer.Value))
                return ParseInputObjectExtension(hasExtend);

            throw new Exception(
                $"Unexpected type definition :'{Encoding.UTF8.GetString(_lexer.Value)}'.");
        }

        public SchemaDefinition ParseSchemaDefinition()
        {
            /* Description? schema Directives? { RootOperationTypeDefinition[] } */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Schema.Span);
            var directives = ParseOptionalDirectives(true);
            var operations = ParseRootOperationDefinitions();

            return new SchemaDefinition(
                description,
                directives,
                operations,
                location);
        }

        public SchemaExtension ParseSchemaExtension(bool hasExtend)
        {
            /* Description? extend schema Directives? { RootOperationTypeDefinition[] } */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Schema.Span);
            var directives = ParseOptionalDirectives(true);
            var operations = ParseOptionalRootOperationDefinitions();

            return new SchemaExtension(
                description,
                directives,
                operations,
                location);
        }

        public IReadOnlyCollection<(OperationType Operation, NamedType NamedType)>? ParseOptionalRootOperationDefinitions()
        {
            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            return ParseRootOperationDefinitions();
        }

        public IReadOnlyCollection<(OperationType Operation, NamedType NamedType)> ParseRootOperationDefinitions()
        {
            Skip(TokenKind.LeftBrace);

            var operations = new List<(OperationType Operation, NamedType NamedType)>();
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                /* OperationType: NamedType */
                if (!Keywords.IsOperation(_lexer.Value, out var operation))
                    throw new Exception(
                        $"Unexpected operation type: '{Encoding.UTF8.GetString(_lexer.Value)}'.");

                Skip(TokenKind.Name);
                Skip(TokenKind.Colon);
                var namedType = ParseNamedType();
                operations.Add((operation, namedType));
            }

            Skip(TokenKind.RightBrace);
            return operations;
        }

        public DirectiveDefinition ParseDirectiveDefinition()
        {
            /* Description? directive @Name ArgumentsDefinition[]? repeatable? on DirectiveLocations*/
            var location = GetLocation();
            var description = ParseOptionalDescription();

            // skip: directive
            SkipKeyword(Keywords.Directive.Span);

            // skip: @
            Skip(TokenKind.At);

            // name
            var name = ParseName();
            var argumentDefinitions = ParseOptionalArgumentDefinitions();

            // repeatable?
            var isRepeatable = false;
            if (Keywords.IsRepeatable(_lexer.Value))
            {
                isRepeatable = true;
                Skip(TokenKind.Name);
            }

            // locations
            var directiveLocations = ParseDirectiveLocations();

            return new DirectiveDefinition(
                description,
                name,
                argumentDefinitions,
                isRepeatable,
                directiveLocations,
                location);
        }

        public IReadOnlyCollection<string> ParseDirectiveLocations()
        {
            /*
            on DirectiveLocations | DirectiveLocation
            on |? DirectiveLocation 
            */

            SkipKeyword(Keywords.On.Span);

            if (_lexer.Kind == TokenKind.Pipe)
                _lexer.Advance();

            var locations = new List<string>(1);
            while (_lexer.Kind == TokenKind.Name)
            {
                var location = Encoding.UTF8.GetString(_lexer.Value);

                var isValid = ExecutableDirectiveLocations.All.Contains(location)
                              || TypeSystemDirectiveLocations.All.Contains(location);

                if (!isValid)
                    break;

                _lexer.Advance();
                locations.Add(location);

                // skip pipe
                if (_lexer.Kind == TokenKind.Pipe)
                    _lexer.Advance();
                else
                    break;
            }

            return locations;
        }

        public IReadOnlyCollection<InputValueDefinition>? ParseOptionalArgumentDefinitions()
        {
            if (_lexer.Kind != TokenKind.LeftParenthesis)
                return null;

            /* (InputValueDefinition[]) */
            Skip(TokenKind.LeftParenthesis);
            var definitions = new List<InputValueDefinition>();
            while (_lexer.Kind != TokenKind.RightParenthesis)
            {
                var definition = ParseInputValueDefinition();
                definitions.Add(definition);
            }

            Skip(TokenKind.RightParenthesis);
            return definitions;
        }

        public InputValueDefinition ParseInputValueDefinition()
        {
            /* Description? Name: Type DefaultValue? Directives? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            var name = ParseName();
            Skip(TokenKind.Colon);
            var type = ParseType();
            var defaultValue = ParseOptionalDefaultValue();
            var directives = ParseOptionalDirectives(true);

            return new InputValueDefinition(
                description,
                name,
                type,
                defaultValue,
                directives,
                location);
        }

        public ScalarDefinition ParseScalarDefinition()
        {
            /* Description? scalar Name Directives? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Scalar.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);

            return new ScalarDefinition(
                description,
                name,
                directives,
                location);
        }

        public TypeExtension ParseScalarExtension(bool hasExtend = true)
        {
            /* Description? extend scalar Name Directives? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Scalar.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);

            return new TypeExtension(new ScalarDefinition(
                description,
                name,
                directives,
                location),
                location);
        }

        public ObjectDefinition ParseObjectDefinition()
        {
            /* Description? type Name ImplementsInterfaces? Directives? FieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Type.Span);
            var name = ParseName();
            var interfaces = ParseOptionalImplementsInterfaces();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalFieldDefinitions();

            return new ObjectDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
                location);
        }

        public TypeExtension ParseObjectExtension(bool hasExtend = true)
        {
            /* Description? extend type Name ImplementsInterfaces? Directives? FieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Type.Span);
            var name = ParseName();
            var interfaces = ParseOptionalImplementsInterfaces();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalFieldDefinitions();

            return new TypeExtension(new ObjectDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
                location),
                location);
        }

        public InterfaceDefinition ParseInterfaceDefinition()
        {
            /* Description interface Name ImplementsInterfaces? Directives? FieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Interface.Span);
            var name = ParseName();
            var interfaces = ParseOptionalImplementsInterfaces();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalFieldDefinitions();

            return new InterfaceDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
                location);
        }

        public TypeExtension ParseInterfaceExtension(bool hasExtend = true)
        {
            /* Description extend interface Name ImplementsInterfaces? Directives? FieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Interface.Span);
            var name = ParseName();
            var interfaces = ParseOptionalImplementsInterfaces();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalFieldDefinitions();

            return new TypeExtension(new InterfaceDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
                location),
                location);
        }

        public UnionDefinition ParseUnionDefinition()
        {
            /* Description? union Name Directives? UnionMemberTypes? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Union.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var members = ParseOptionalUnionMembers();

            return new UnionDefinition(
                description,
                name,
                directives,
                members,
                location);
        }

        public TypeExtension ParseUnionExtension(bool hasExtend = true)
        {
            /* Description? extend union Name Directives? UnionMemberTypes? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Union.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var members = ParseOptionalUnionMembers();

            return new TypeExtension(new UnionDefinition(
                description,
                name,
                directives,
                members,
                location),
                location);
        }

        public EnumDefinition ParseEnumDefinition()
        {
            /* Description? enum Name Directives? EnumValuesDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Enum.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var values = ParseOptionalEnumValueDefinitions();

            return new EnumDefinition(
                description,
                name,
                directives,
                values,
                location);
        }

        public TypeExtension ParseEnumExtension(bool hasExtend = true)
        {
            /* Description? extend enum Name Directives? EnumValuesDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Enum.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var values = ParseOptionalEnumValueDefinitions();

            return new TypeExtension(new EnumDefinition(
                description,
                name,
                directives,
                values,
                location),
                location);
        }

        public IReadOnlyCollection<EnumValueDefinition>? ParseOptionalEnumValueDefinitions()
        {
            /* { EnumValueDefinition[] } */
            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            Skip(TokenKind.LeftBrace);

            var values = new List<EnumValueDefinition>();
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var value = ParseEnumValueDefinition();
                values.Add(value);
            }

            Skip(TokenKind.RightBrace);
            return values;
        }

        public EnumValueDefinition ParseEnumValueDefinition()
        {
            /* Description? EnumValue Directives? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            var value = ParseEnumValue();
            var directives = ParseOptionalDirectives(true);

            return new EnumValueDefinition(
                description,
                value,
                directives,
                location);
        }

        public InputObjectDefinition ParseInputObjectDefinition()
        {
            /* Description? input Name Directives? InputFieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Input.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalInputObjectFields();

            return new InputObjectDefinition(
                description,
                name,
                directives,
                fields,
                location);
        }

        public TypeExtension ParseInputObjectExtension(bool hasExtend = true)
        {
            /* Description? extend input Name Directives? InputFieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Extend.Span, optional: !hasExtend);
            SkipKeyword(Keywords.Input.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(true);
            var fields = ParseOptionalInputObjectFields();

            return new TypeExtension(new InputObjectDefinition(
                description,
                name,
                directives,
                fields,
                location),
                location);
        }

        public FieldDefinition ParseFieldDefinition()
        {
            /* Description? Name ArgumentsDefinition?:Type Directives? */

            var location = GetLocation();
            var description = ParseOptionalDescription();
            var name = ParseName();
            var argumentDefinitions = ParseOptionalArgumentDefinitions();
            Skip(TokenKind.Colon);
            var type = ParseType();
            var directives = ParseOptionalDirectives(true);


            return new FieldDefinition(
                description,
                name,
                argumentDefinitions,
                type,
                directives,
                location);
        }

        internal IReadOnlyCollection<InputValueDefinition>? ParseOptionalInputObjectFields()
        {
            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            Skip(TokenKind.LeftBrace);

            var values = new List<InputValueDefinition>(5);
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var value = ParseInputValueDefinition();
                values.Add(value);
            }

            Skip(TokenKind.RightBrace);
            return values;
        }

        internal IReadOnlyCollection<NamedType>? ParseOptionalUnionMembers()
        {
            /*  UnionMemberTypes | NamedType
                = |? NamedType 
            */

            if (_lexer.Kind != TokenKind.Equal)
                return null;

            Skip(TokenKind.Equal);

            if (_lexer.Kind == TokenKind.Pipe)
                _lexer.Advance();

            var namedTypes = new List<NamedType>(2);
            while (_lexer.Kind == TokenKind.Name)
            {
                var nameType = ParseNamedType();
                namedTypes.Add(nameType);

                if (_lexer.Kind == TokenKind.Pipe)
                    _lexer.Advance();
                else
                    break;
            }

            return namedTypes;
        }

        internal IReadOnlyCollection<FieldDefinition>? ParseOptionalFieldDefinitions()
        {
            /* { FieldDefinition } */

            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            Skip(TokenKind.LeftBrace);

            var fields = new List<FieldDefinition>(5);
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var field = ParseFieldDefinition();
                fields.Add(field);
            }

            Skip(TokenKind.RightBrace);

            return fields;
        }

        internal IReadOnlyCollection<NamedType>? ParseOptionalImplementsInterfaces()
        {
            /*  ImplementsInterfaces & NamedType
                implements &? NamedType
            */

            if (!Keywords.IsImplements(_lexer.Value))
                return null;

            SkipKeyword(Keywords.Implements.Span);

            // skip &
            if (_lexer.Kind == TokenKind.Ampersand)
                _lexer.Advance();

            var namedTypes = new List<NamedType>();
            while (_lexer.Kind == TokenKind.Name)
            {
                var namedType = ParseNamedType();
                namedTypes.Add(namedType);

                // skip &
                if (_lexer.Kind == TokenKind.Ampersand)
                    _lexer.Advance();
                else
                    break;
            }

            return namedTypes;
        }

        public StringValue? ParseOptionalDescription()
        {
            // use preparsed description if it has
            // been cached
            if (_description != null)
            {
                var value = _description;
                _description = null;
                return value;
            }

            if (_lexer.Kind != TokenKind.StringValue
                && _lexer.Kind != TokenKind.BlockStringValue)
                return null;

            if (_lexer.Kind == TokenKind.BlockStringValue)
                return ParseBlockStringValue();

            return ParseStringValue();
        }

        private void PreParseOptionalDescription()
        {
            _description = ParseOptionalDescription();
        }
    }
}