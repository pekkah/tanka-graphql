using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Tanka.GraphQL.Language.Internal;
using Tanka.GraphQL.Language.Nodes;
using Type = Tanka.GraphQL.Language.Nodes.Type;

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

        public Document ParseDocument()
        {
            var operations = new List<OperationDefinition>();
            while (_lexer.Kind != TokenKind.End)
            {
                _lexer.Advance();
                switch (_lexer.Kind)
                {
                    case TokenKind.Name:
                        if (Keywords.IsOperation(_lexer.Value, out var operationType))
                            operations.Add(ParseOperationDefinition(operationType));
                        break;
                    case TokenKind.LeftBrace:
                        operations.Add(ParseShortOperationDefinition());
                        break;
                    default:
                        throw new Exception($"Unexpected token {_lexer.Kind} at {_lexer.Line}:{_lexer.Column}");
                }
            }


            return new Document(operations);
        }

        public OperationDefinition ParseOperationDefinition(in OperationType operationType)
        {
            var location = GetLocation();
            // OperationType Name? VariableDefinitions? Directives? SelectionSet

            // OperationType (coming in as param)
            Skip(TokenKind.Name);

            var name = ParseOptionalName();
            var variableDefinitions = ParseOptionalVariableDefinitions();
            var directives = ParseOptionalDirectives();
            var selectionSet = ParseSelectionSet();

            return new OperationDefinition(
                in operationType,
                in name,
                in variableDefinitions,
                in directives,
                in selectionSet,
                in location);
        }

        public OperationDefinition ParseShortOperationDefinition()
        {
            var selectionSet = ParseSelectionSet();

            return new OperationDefinition(
                OperationType.Query,
                null,
                null,
                null,
                in selectionSet,
                in selectionSet.Location);
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
                in variable,
                in type,
                in defaultValue,
                in directives,
                in variable.Location);
        }

        public IReadOnlyCollection<Directive>? ParseOptionalDirectives(in bool constant = false)
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

        public Directive ParseDirective(in bool constant = false)
        {
            /* @Name Arguments? */
            var location = Skip(TokenKind.At);
            var name = ParseName();
            var arguments = ParseOptionalArguments(constant);

            return new Directive(
                in name,
                in arguments,
                in location);
        }

        public IReadOnlyCollection<Argument>? ParseOptionalArguments(in bool constant = false)
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

        public Argument ParseArgument(in bool constant = false)
        {
            /* Name:Value */
            var name = ParseName();
            Skip(TokenKind.Colon);
            var value = ParseValue(in constant);

            return new Argument(
                in name,
                in value,
                in name.Location);
        }

        public DefaultValue? ParseOptionalDefaultValue()
        {
            if (_lexer.Kind != TokenKind.Equal)
                return null;

            var location = Skip(TokenKind.Equal);
            var value = ParseValue(true);

            return new DefaultValue(value, location);
        }

        public IValue ParseValue(in bool constant = false)
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
                return new NullValue(in location);
            }

            if (Keywords.IsBoolean(_lexer.Value, out var value))
            {
                Skip(TokenKind.Name);
                return new BooleanValue(in value, in location);
            }

            //todo: maybe this should be kept as byte[]?
            var enumName = ParseName();
            return new EnumValue(in enumName, in location);
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
            if (!Utf8Parser.TryParse(_lexer.Value, out double floater, out _))
                throw new Exception("Could not parse float value");

            var location = Skip(TokenKind.FloatValue);
            return new FloatValue(floater, location);
        }

        public IntValue ParseIntValue()
        {
            if (!Utf8Parser.TryParse(_lexer.Value, out int integer, out _))
                throw new Exception("Could not parse integer value");

            var location = Skip(TokenKind.IntValue);
            return new IntValue(integer, location);
        }

        public ObjectValue ParseObjectValue(in bool constant = false)
        {
            var location = Skip(TokenKind.LeftBrace);
            var fields = new List<ObjectField>();

            while (_lexer.Kind != TokenKind.RightBrace)
            {
                var field = ParseObjectField(constant);
                fields.Add(field);
            }

            Skip(TokenKind.RightBrace);
            return new ObjectValue(fields, in location);
        }

        public ObjectField ParseObjectField(in bool constant = false)
        {
            var name = ParseName();
            Skip(TokenKind.Colon);
            var value = ParseValue(in constant);

            return new ObjectField(in name, in value, in name.Location);
        }

        public ListValue ParseListValue(in bool constant)
        {
            var location = Skip(TokenKind.LeftBracket);

            var values = new List<IValue>();
            while (_lexer.Kind != TokenKind.RightBracket)
            {
                var value = ParseValue(in constant);
                values.Add(value);
            }

            Skip(TokenKind.RightBracket);
            return new ListValue(values, in location);
        }

        public Type ParseType()
        {
            // Type
            // [Type]
            // Type!
            // [Type!]
            // [Type]!

            // [Type]
            var location = GetLocation();
            Type type;
            if (_lexer.Kind == TokenKind.LeftBracket)
            {
                Skip(TokenKind.LeftBracket);
                var listType = ParseType();
                Skip(TokenKind.RightBracket);
                type = new ListOf(listType, location);
            }
            else
            {
                type = ParseNamedType();
            }

            // Type!
            if (_lexer.Kind == TokenKind.ExclamationMark)
            {
                Skip(TokenKind.ExclamationMark);
                type = new NonNullOf(type, location);
            }

            return type;
        }

        public NamedType ParseNamedType()
        {
            var name = ParseName();
            return new NamedType(in name, in name.Location);
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
                in name,
                in arguments,
                in directives,
                in selectionSet,
                in location);
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
                in name,
                in directives,
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
                in typeCondition,
                in directives,
                in selectionSet,
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

            if (!Keywords.IsOn(_lexer.Value))
                throw new Exception(
                    $"Unexpected keyword '{Encoding.UTF8.GetString(_lexer.Value)}'. " +
                    "Expected 'on'.");

            // on
            Skip(TokenKind.Name);

            return ParseNamedType();
        }

        private Name? ParseOptionalName()
        {
            SkipComment();

            if (_lexer.Kind != TokenKind.Name)
                return null;

            return ParseName();
        }

        private Name ParseName()
        {
            Ensure(TokenKind.Name);

            var value = Encoding.UTF8.GetString(_lexer.Value);
            var location = GetLocation();
            _lexer.Advance();
            return new Name(in value, in location);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location Ensure(TokenKind kind)
        {
            SkipComment();

            if (_lexer.Kind != kind)
                throw new Exception(
                    $"Unexpected token: {_lexer.Kind}@{_lexer.Line}:{_lexer.Column}. " +
                    $"Expected: {kind}");

            return GetLocation();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location GetLocation()
        {
            return new Location(_lexer.Line, _lexer.Column);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipComment()
        {
            if (_lexer.Kind == TokenKind.Comment)
                _lexer.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Location Skip(TokenKind expectedToken)
        {
            var location = Ensure(expectedToken);

            if (!_lexer.Advance() && _lexer.Kind != TokenKind.End)
                throw new Exception(
                    $"Expected to skip {expectedToken} at {_lexer.Line}:{_lexer.Column} but lexer could not advance");

            return location;
        }

        private string BlockStringValue(in ReadOnlySpan<byte> value)
        {
            var reader = new BlockStringValueReader(in value);
            var blockStringValue = reader.Read();
            return Encoding.UTF8.GetString(blockStringValue);
        }
    }
}