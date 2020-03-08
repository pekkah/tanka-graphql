using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public ref struct Parser
    {
        private Lexer _lexer;
        
        public Parser(in ReadOnlySpan<byte> span)
        {
            _lexer = Lexer.Create(span);
        }

        public static Parser Create(in ReadOnlySpan<byte> span)
        {
            return new Parser(span);
        }

        public static Parser Create(in string data)
        {
            return Create(Encoding.UTF8.GetBytes(data));
        }

        public Document Parse()
        {
            var operations = new List<OperationDefinition>();
            while (_lexer.Advance())
            {
                switch (_lexer.Kind)
                {
                    case TokenKind.Name:
                        if (Keywords.IsOperation(_lexer.Value, out var operationType))
                        {
                            ParseOperationDefinition(operations, operationType);
                        }
                        break;
                }

                if (_lexer.Kind == TokenKind.End)
                    break;

                throw new Exception($"Unexpected token {_lexer.Kind} at {_lexer.Line}:{_lexer.Column}");
            }


            return new Document(operations.ToArray());
        }

        private void ParseOperationDefinition(List<OperationDefinition> operations, in OperationType operationType)
        {
            var location = GetLocation();
            // OperationType Name? VariableDefinitions? Directives? SelectionSet

            // OperationType (coming in as arg
            Skip(TokenKind.Name);

            // Name?
            var name = ParseName();

            //var variableDefinitions = ParseVariableDefinitions();
            //var directives = ParseDirectives();
            var selectionSet = ParseSelectionSet();

            var operation = new OperationDefinition(
                in operationType, 
                in name, 
                in selectionSet, 
                in location);
            
            operations.Add(operation);
        }

        private SelectionSet ParseSelectionSet()
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
            var selections = new List<Selection>();
            while (_lexer.Kind != TokenKind.RightBrace)
            {
                if (_lexer.Kind == TokenKind.RightBrace)
                    break;

                // check for fragment
                if (_lexer.Kind == TokenKind.Spread)
                    throw new NotImplementedException();

                var field = ParseField();
                selections.Add(field);
            }

            // }
            Skip(TokenKind.RightBrace);

            return new SelectionSet(selections.ToArray(), location);
        }

        private Selection ParseField()
        {
            return default;
        }

        private Name? ParseName()
        {
            if (_lexer.Kind != TokenKind.Name)
                return null;

            var value = Encoding.UTF8.GetString(_lexer.Value);
            var location = GetLocation();

            return new Name(in value, in location);
        }

        private Location GetLocation()
        {
            return new Location(_lexer.Line, _lexer.Column);
        }

        private void Skip(TokenKind expectedToken)
        {
            if (_lexer.Kind != expectedToken)
                throw new Exception(
                    $"Expected to skip {expectedToken} at {_lexer.Line}:{_lexer.Column} but current is {_lexer.Kind}");
            
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