using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Tanka.GraphQL.Language.Internal;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public ref struct Parser
    {
        private Lexer _lexer;

        public Parser(in ReadOnlySpan<byte> span)
        {
            _lexer = Lexer.Create(span);
            _lexer.Advance();
        }

        public static Parser Create(in ReadOnlySpan<byte> span)
        {
            return new Parser(span);
        }

        public static Parser Create(in string data)
        {
            return Create(Encoding.UTF8.GetBytes(data));
        }

        public TypeSystemDocument ParseTypeDefinitions()
        {
            return new TypeSystemDocument(
                null,
                null,
                null,
                null,
                null);
        }

        public SchemaDefinition ParseSchemaDefinition()
        {
            /* Description? schema Directives? { RootOperationTypeDefinition[] } */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Schema.Span);
            var directives = ParseOptionalDirectives(constant: true);
            var operations = ParseRootOperationDefinitions();
            
            return new SchemaDefinition(
                description,
                directives,
                operations,
                location);
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
            var directives = ParseOptionalDirectives(constant: true);

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
            var directives = ParseOptionalDirectives(constant: true);

            return new ScalarDefinition(
                description,
                name,
                directives,
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
            var directives = ParseOptionalDirectives(constant: true);
            var fields = ParseOptionalFieldDefinitions();

            return new ObjectDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
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
            var directives = ParseOptionalDirectives(constant: true);
            var fields = ParseOptionalFieldDefinitions();

            return new InterfaceDefinition(
                description,
                name,
                interfaces,
                directives,
                fields,
                location);
        }

        public UnionDefinition ParseUnionDefinition()
        {
            /* Description? union Name Directives? UnionMemberTypes? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Union.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(constant: true);
            var members = ParseOptionalUnionMembers();

            return new UnionDefinition(
                description,
                name,
                directives,
                members,
                location);
        }

        public EnumDefinition ParseEnumDefinition()
        {
            /* Description? enum Name Directives? EnumValuesDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Enum.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(constant: true);
            var values = ParseOptionalEnumValueDefinitions();

            return new EnumDefinition(
                description,
                name,
                directives,
                values,
                location);
        }

        internal IReadOnlyCollection<EnumValueDefinition>? ParseOptionalEnumValueDefinitions()
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
            var directives = ParseOptionalDirectives(constant: true);

            return new EnumValueDefinition(
                description,
                value,
                directives,
                location);
        }

        public InputObjectDefinition ParseInputObjectDefinition()
        {
            /* Description input Name Directives? InputFieldsDefinition? */
            var location = GetLocation();
            var description = ParseOptionalDescription();
            SkipKeyword(Keywords.Input.Span);
            var name = ParseName();
            var directives = ParseOptionalDirectives(constant: true);
            var fields = ParseOptionalInputObjectFields();

            return new InputObjectDefinition(
                description,
                name,
                directives,
                fields,
                location);
        }

        internal IReadOnlyCollection<InputValueDefinition>? ParseOptionalInputObjectFields()
        {
            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            Skip(TokenKind.LeftBrace);

            var values = new List<InputValueDefinition>();
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

            var namedTypes = new List<NamedType>();
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

            var fields = new List<FieldDefinition>(1);
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var field = ParseFieldDefinition();
                fields.Add(field);
            }

            Skip(TokenKind.RightBrace);

            return fields;
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
            var directives = ParseOptionalDirectives(constant: true);


            return new FieldDefinition(
                description,
                name,
                argumentDefinitions,
                type,
                directives,
                location);
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


        private Location SkipKeyword(in ReadOnlySpan<byte> keyword)
        {
            if (_lexer.Kind != TokenKind.Name)
                throw new Exception($"Unexpected token: '{_lexer.Kind}'. " +
                                    $"Expected: '{TokenKind.Name}'");

            if (!keyword.SequenceEqual(_lexer.Value))
                throw new Exception(
                    $"Unexpected keyword: '{Encoding.UTF8.GetString(_lexer.Value)}'. " +
                    $"Expected: '{Encoding.UTF8.GetString(keyword)}'.");

            return Skip(TokenKind.Name);
        }

        public StringValue? ParseOptionalDescription()
        {
            if (_lexer.Kind != TokenKind.StringValue 
                && _lexer.Kind != TokenKind.BlockStringValue)
                return null;

            if (_lexer.Kind == TokenKind.BlockStringValue)
                return ParseBlockStringValue();

            return ParseStringValue();
        }

        public ExecutableDocument ParseExecutableDocument()
        {
            var operations = new List<OperationDefinition>(1);
            var fragmentDefinitions = new List<FragmentDefinition>();
            while (_lexer.Kind != TokenKind.End)
            {
                switch (_lexer.Kind)
                {
                    case TokenKind.Name:
                        if (Keywords.IsOperation(_lexer.Value, out var operationType))
                            operations.Add(ParseOperationDefinition(operationType));
                        else if (Keywords.IsFragment(_lexer.Value))
                            fragmentDefinitions.Add(ParseFragmentDefinition());
                        break;
                    case TokenKind.LeftBrace:
                        operations.Add(ParseShortOperationDefinition());
                        break;
                    default:
                        throw new Exception($"Unexpected token {_lexer.Kind} at {_lexer.Line}:{_lexer.Column}");
                }

                _lexer.Advance();
            }


            return new ExecutableDocument(
                operations,
                fragmentDefinitions);
        }

        public FragmentDefinition ParseFragmentDefinition()
        {
            /* fragment FragmentName TypeCondition Directives? SelectionSet */
            if (!Keywords.IsFragment(_lexer.Value))
                throw new Exception("Unexpected keyword. Expected 'fragment'.");

            // fragment
            var location = Skip(TokenKind.Name);

            var fragmentName = ParseFragmentName();
            var typeCondition = ParseTypeCondition();
            var directives = ParseOptionalDirectives();
            var selectionSet = ParseSelectionSet();

            return new FragmentDefinition(
                fragmentName,
                typeCondition,
                directives,
                selectionSet,
                location);
        }

        public OperationDefinition ParseOperationDefinition(OperationType operationType)
        {
            // OperationType Name? VariableDefinitions? Directives? SelectionSet

            // OperationType (coming as param)
            var location = Skip(TokenKind.Name);

            var name = ParseOptionalName();
            var variableDefinitions = ParseOptionalVariableDefinitions();
            var directives = ParseOptionalDirectives();
            var selectionSet = ParseSelectionSet();

            return new OperationDefinition(
                operationType,
                name,
                variableDefinitions,
                directives,
                selectionSet,
                location);
        }

        public OperationDefinition ParseShortOperationDefinition()
        {
            var selectionSet = ParseSelectionSet();

            return new OperationDefinition(
                OperationType.Query,
                null,
                null,
                null,
                selectionSet,
                selectionSet.Location);
        }

        public IReadOnlyCollection<VariableDefinition>? ParseOptionalVariableDefinitions()
        {
            SkipComment();

            if (_lexer.Kind != TokenKind.LeftParenthesis)
                return null;

            return ParseVariableDefinitions();
        }

        public IReadOnlyCollection<VariableDefinition> ParseVariableDefinitions()
        {
            // (VariableDefinition[])
            Skip(TokenKind.LeftParenthesis);

            var variableDefinitions = new List<VariableDefinition>();
            while (_lexer.Kind != TokenKind.RightParenthesis)
            {
                SkipComment();

                var variableDefinition = ParseVariableDefinition();
                variableDefinitions.Add(variableDefinition);
            }

            Skip(TokenKind.RightParenthesis);

            return variableDefinitions;
        }

        public VariableDefinition ParseVariableDefinition()
        {
            /* Variable: Type DefaultValue? Directives? */
            var variable = ParseVariable();
            Skip(TokenKind.Colon);
            var type = ParseType();
            var defaultValue = ParseOptionalDefaultValue();
            var directives = ParseOptionalDirectives();

            return new VariableDefinition(
                variable,
                type,
                defaultValue,
                directives,
                variable.Location);
        }

        public IReadOnlyCollection<Directive>? ParseOptionalDirectives(bool constant = false)
        {
            if (_lexer.Kind != TokenKind.At)
                return null;

            var directives = new List<Directive>();
            while (_lexer.Kind == TokenKind.At)
            {
                var directive = ParseDirective(constant);
                directives.Add(directive);
            }

            return directives;
        }

        public Directive ParseDirective(bool constant = false)
        {
            /* @Name Arguments? */
            var location = Skip(TokenKind.At);
            var name = ParseName();
            var arguments = ParseOptionalArguments(constant);

            return new Directive(
                name,
                arguments,
                location);
        }

        public IReadOnlyCollection<Argument>? ParseOptionalArguments(bool constant = false)
        {
            /* (Argument[]) */
            if (_lexer.Kind != TokenKind.LeftParenthesis)
                return null;

            Skip(TokenKind.LeftParenthesis);
            var arguments = new List<Argument>();

            while (_lexer.Kind != TokenKind.RightParenthesis)
            {
                var argument = ParseArgument(constant);
                arguments.Add(argument);
            }

            Skip(TokenKind.RightParenthesis);
            return arguments;
        }

        public Argument ParseArgument(bool constant = false)
        {
            /* Name:Value */
            var name = ParseName();
            Skip(TokenKind.Colon);
            var value = ParseValue(constant);

            return new Argument(
                name,
                value,
                name.Location);
        }

        public DefaultValue? ParseOptionalDefaultValue()
        {
            if (_lexer.Kind != TokenKind.Equal)
                return null;

            var location = Skip(TokenKind.Equal);
            var value = ParseValue(true);

            return new DefaultValue(value, location);
        }

        public IValue ParseValue(bool constant = false)
        {
            /* Value :
                if not const Variable
                IntValue
                FloatValue
                StringValue
                BooleanValue
                NullValue
                EnumValue
                ListValue [const]
                ObjectValue {const}
            */

            return (_lexer.Kind, constant) switch
            {
                (TokenKind.Dollar, false) => ParseVariable(),
                (TokenKind.Dollar, true) => throw new Exception("Unexpected variable on constants value"),
                (TokenKind.IntValue, _) => ParseIntValue(),
                (TokenKind.FloatValue, _) => ParseFloatValue(),
                (TokenKind.StringValue, _) => ParseStringValue(),
                (TokenKind.BlockStringValue, _) => ParseBlockStringValue(),
                (TokenKind.Name, _) => ParseNameValue(), // boolean or enum or null
                (TokenKind.LeftBracket, _) => ParseListValue(constant),
                (TokenKind.LeftBrace, _) => ParseObjectValue(constant),
                _ => throw new Exception($"Unexpected value token: {_lexer.Kind}")
            };
        }

        /// <summary>
        ///     Parse NullValue, BooleanValue or EnumValue
        /// </summary>
        /// <returns></returns>
        public IValue ParseNameValue()
        {
            var location = Ensure(TokenKind.Name);
            if (Keywords.IsNull(_lexer.Value))
            {
                Skip(TokenKind.Name);
                return new NullValue(location);
            }

            if (Keywords.IsBoolean(_lexer.Value, out var value))
            {
                Skip(TokenKind.Name);
                return new BooleanValue(value, location);
            }

            return ParseEnumValue();
        }

        public EnumValue ParseEnumValue()
        {
            //todo: maybe this should be kept as byte[]?
            var enumName = ParseName();
            return new EnumValue(enumName, enumName.Location);
        }

       public StringValue ParseBlockStringValue()
       {
            Ensure(TokenKind.BlockStringValue);
            var value = BlockStringValue(_lexer.Value);
            var location = Skip(TokenKind.BlockStringValue);
            return new StringValue(value, location);
        }

        public StringValue ParseStringValue()
        {
            var value = Encoding.UTF8.GetString(_lexer.Value);
            var location = Skip(TokenKind.StringValue);
            return new StringValue(value, location);
        }

        public FloatValue ParseFloatValue()
        {
            var value = new FloatValue(_lexer.Value, GetLocation());
            Skip(TokenKind.FloatValue);
            return value;
        }

        public IntValue ParseIntValue()
        {
            if (!Utf8Parser.TryParse(_lexer.Value, out int integer, out _))
                throw new Exception("Could not parse integer value");

            var location = Skip(TokenKind.IntValue);
            return new IntValue(integer, location);
        }

        public ObjectValue ParseObjectValue(bool constant = false)
        {
            var location = Skip(TokenKind.LeftBrace);
            var fields = new List<ObjectField>();

            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var field = ParseObjectField(constant);
                fields.Add(field);
            }

            Skip(TokenKind.RightBrace);
            return new ObjectValue(fields, location);
        }

        public ObjectField ParseObjectField(bool constant = false)
        {
            var name = ParseName();
            Skip(TokenKind.Colon);
            var value = ParseValue(constant);

            return new ObjectField(name, value, name.Location);
        }

        public ListValue ParseListValue(bool constant)
        {
            var location = Skip(TokenKind.LeftBracket);

            var values = new List<IValue>();
            while (_lexer.Kind != TokenKind.RightBracket)
            {
                var value = ParseValue(constant);
                values.Add(value);
            }

            Skip(TokenKind.RightBracket);
            return new ListValue(values, location);
        }

        public IType ParseType()
        {
            // Type
            // [Type]
            // Type!
            // [Type!]
            // [Type]!

            // [Type]
            var location = GetLocation();
            IType type;
            if (_lexer.Kind == TokenKind.LeftBracket)
            {
                Skip(TokenKind.LeftBracket);
                var listType = ParseType();
                Skip(TokenKind.RightBracket);
                type = new ListType(listType, location);
            }
            else
            {
                type = ParseNamedType();
            }

            // Type!
            if (_lexer.Kind == TokenKind.ExclamationMark)
            {
                Skip(TokenKind.ExclamationMark);
                type = new NonNullType(type, location);
            }

            return type;
        }

        public NamedType ParseNamedType()
        {
            var name = ParseName();
            return new NamedType(name, name.Location);
        }

        public Variable ParseVariable()
        {
            var location = Skip(TokenKind.Dollar);
            return new Variable(ParseName(), location);
        }

        public SelectionSet? ParseOptionalSelectionSet()
        {
            SkipComment();

            if (_lexer.Kind != TokenKind.LeftBrace)
                return null;

            return ParseSelectionSet();
        }

        public SelectionSet ParseSelectionSet()
        {
            /*  SelectionSet
                    {Selection[]}

                Selection
                    Field
                    FragmentSpread
                    InlineFragment
            */
            var location = GetLocation();

            // {
            Skip(TokenKind.LeftBrace);

            // parse until }
            var selections = new List<ISelection>();
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                SkipComment();
                ISelection selection;

                // check for fragment
                if (_lexer.Kind == TokenKind.Spread)
                {
                    Skip(TokenKind.Spread);
                    Ensure(TokenKind.Name);
                    if (Keywords.IsOn(_lexer.Value))
                        selection = ParseInlineFragment(false);
                    else
                        selection = ParseFragmentSpread(false);
                }
                else
                {
                    // field selection
                    selection = ParseFieldSelection();
                }

                // add selection
                selections.Add(selection);
            }

            // }
            Skip(TokenKind.RightBrace);

            return new SelectionSet(selections, location);
        }

        public FieldSelection ParseFieldSelection()
        {
            // Alias? Name Arguments? Directives? SelectionSet?
            var location = GetLocation();
            var nameOrAlias = ParseName();
            var name = nameOrAlias;

            var hasAlias = false;
            if (_lexer.Kind == TokenKind.Colon)
            {
                _lexer.Advance();
                name = ParseName();
                hasAlias = true;
            }

            var arguments = ParseOptionalArguments();
            var directives = ParseOptionalDirectives();
            var selectionSet = ParseOptionalSelectionSet();

            return new FieldSelection(
                hasAlias ? nameOrAlias : null,
                name,
                arguments,
                directives,
                selectionSet,
                location);
        }

        public FragmentSpread ParseFragmentSpread(bool spread = true)
        {
            /* ...FragmentName Directives? */
            Location? location = null;

            //todo: location might be wrong (spread skipped when spread == false)
            if (spread)
                location = Skip(TokenKind.Spread);

            var name = ParseFragmentName();

            var directives = ParseOptionalDirectives();

            return new FragmentSpread(
                name,
                directives,
                location ?? name.Location);
        }

        public InlineFragment ParseInlineFragment(bool spread = true)
        {
            /* ... TypeCondition? Directives? SelectionSet */
            Location? location = null;
            if (spread)
                location = Skip(TokenKind.Spread);

            var typeCondition = ParseOptionalTypeCondition();
            var directives = ParseOptionalDirectives();
            var selectionSet = ParseSelectionSet();

            return new InlineFragment(
                typeCondition,
                directives,
                selectionSet,
                location ?? selectionSet.Location);
        }

        public Name ParseFragmentName()
        {
            /* Name but not on */
            if (Keywords.IsOn(_lexer.Value))
                throw new Exception("Unexpected keyword on");

            return ParseName();
        }

        private NamedType? ParseOptionalTypeCondition()
        {
            /* on NamedType */
            if (_lexer.Kind != TokenKind.Name)
                return null;

            return ParseTypeCondition();
        }

        public NamedType ParseTypeCondition()
        {
            if (!Keywords.IsOn(_lexer.Value))
                throw new Exception(
                    $"Unexpected keyword '{Encoding.UTF8.GetString(_lexer.Value)}'. " +
                    "Expected 'on'.");

            // on
            Skip(TokenKind.Name);

            return ParseNamedType();
        }

        public Name? ParseOptionalName()
        {
            SkipComment();

            if (_lexer.Kind != TokenKind.Name)
                return null;

            return ParseName();
        }

        public Name ParseName()
        {
            var location = Ensure(TokenKind.Name);

            var value = Encoding.UTF8.GetString(_lexer.Value);
            _lexer.Advance();

            return new Name(value, location);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location Ensure(TokenKind kind)
        {
            SkipComment();

            if (_lexer.Kind != kind)
                throw new Exception(
                    $"Unexpected token: {_lexer.Kind}@{_lexer.Line}:{_lexer.Column}. " +
                    $"Expected: {kind}");

            return GetLocation();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location GetLocation()
        {
            return new Location(_lexer.Line, _lexer.Column);
        }

        [Conditional("GQL_COMMENTS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipComment()
        {
#if GQL_COMMENTS
            if (_lexer.Kind == TokenKind.Comment)
                _lexer.Advance();
#endif
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location Skip(TokenKind expectedToken)
        {
            var location = Ensure(expectedToken);

            if (!_lexer.Advance() && _lexer.Kind != TokenKind.End)
                throw new Exception(
                    $"Expected to skip {expectedToken} at {_lexer.Line}:{_lexer.Column} but lexer could not advance");

            return location;
        }

        private string BlockStringValue(ReadOnlySpan<byte> value)
        {
            var reader = new BlockStringValueReader(value);
            var blockStringValue = reader.Read();
            return Encoding.UTF8.GetString(blockStringValue);
        }
    }
}