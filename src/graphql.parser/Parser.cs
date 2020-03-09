using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

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
                }

                throw new Exception($"Unexpected token {_lexer.Kind} at {_lexer.Line}:{_lexer.Column}");
            }


            return new Document(operations);
        }

        public OperationDefinition ParseOperationDefinition(in OperationType operationType)
        {
            var location = GetLocation();
            // OperationType Name? VariableDefinitions? Directives? SelectionSet

            // OperationType (coming in as param)
            Skip(TokenKind.Name);

            // Name?
            var name = ParseOptionalName();

            //var variableDefinitions = ParseVariableDefinitions();
            //var directives = ParseDirectives();
            var selectionSet = ParseSelectionSet();

            return new OperationDefinition(
                in operationType,
                in name,
                in selectionSet,
                in location);
        }

        public SelectionSet? ParseOptionalSelectionSet()
        {
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
            var selections = new List<FieldSelection>();
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                // check for fragment
                if (_lexer.Kind == TokenKind.Spread)
                    throw new NotImplementedException();

                var field = ParseFieldSelection();
                selections.Add(field);
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

            if (name == null)
                throw new Exception("Field must have name");

            //var arguments = ParseArguments();
            //var directives = ParseDirectives();
            var selectionSet = ParseOptionalSelectionSet();

            return new FieldSelection(
                hasAlias ? nameOrAlias : null,
                in name,
                in selectionSet,
                in location);
        }

        private Name? ParseOptionalName()
        {
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

        private void Ensure(TokenKind kind)
        {
            if (_lexer.Kind != kind)
                throw new Exception(
                    $"Unexpected token: {_lexer.Kind}@{_lexer.Line}:{_lexer.Column}. " +
                    $"Expected: {kind}");
        }

        private Location GetLocation()
        {
            return new Location(_lexer.Line, _lexer.Column);
        }

        private void Skip(TokenKind expectedToken)
        {
            Ensure(expectedToken);

            if (!_lexer.Advance() && _lexer.Kind != TokenKind.End)
                throw new Exception(
                    $"Expected to skip {expectedToken} at {_lexer.Line}:{_lexer.Column} but lexer could not advance");
        }
    }

    public static class Keywords
    {
        public static ReadOnlyMemory<byte> Query
            = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("query"));

        public static bool IsOperation(ReadOnlySpan<byte> value, out OperationType operation)
        {
            if (Query.Span.SequenceEqual(value))
            {
                operation = OperationType.Query;
                return true;
            }

            operation = default;
            return false;
        }
    }
}