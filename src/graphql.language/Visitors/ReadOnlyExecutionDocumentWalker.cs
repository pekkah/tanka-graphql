using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language.Visitors
{
    public sealed class ReadOnlyExecutionDocumentWalker
    {
        private readonly ExecutionDocumentWalkerOptions _options;

        public ReadOnlyExecutionDocumentWalker(ExecutionDocumentWalkerOptions options)
        {
            _options = options;
        }

        public void Visit(ExecutableDocument document)
        {
            // enter
            foreach (var visitor in _options.ExecutableDocument)
                visitor.Enter(document);

            // children
            if (document.FragmentDefinitions != null)
                foreach (var definition in document.FragmentDefinitions)
                    Visit(definition);

            if (document.OperationDefinitions != null)
                foreach (var definition in document.OperationDefinitions)
                    Visit(definition);

            // leave
            foreach (var visitor in _options.ExecutableDocument) visitor.Leave(document);
        }

        public void Visit(FragmentDefinition definition)
        {
            // enter
            foreach (var visitor in _options.FragmentDefinition)
                visitor.Enter(definition);

            // children
            Visit(definition.Directives);
            Visit(definition.SelectionSet);

            // leave
            foreach (var visitor in _options.FragmentDefinition)
                visitor.Leave(definition);
        }

        public void Visit(SelectionSet? selectionSet)
        {
            if (selectionSet == null)
                return;

            // enter
            foreach (var visitor in _options.SelectionSet)
                visitor.Enter(selectionSet);

            foreach (var selection in selectionSet.Selections) Visit(selection);

            // leave
            foreach (var visitor in _options.SelectionSet)
                visitor.Leave(selectionSet);
        }

        public void Visit(ISelection selection)
        {
            // enter
            foreach (var visitor in _options.Selection)
                visitor.Enter(selection);

            // children
            switch (selection.SelectionType)
            {
                case SelectionType.Field:
                    Visit((FieldSelection) selection);
                    break;
                case SelectionType.InlineFragment:
                    Visit((InlineFragment) selection);
                    break;
                case SelectionType.FragmentSpread:
                    Visit((FragmentSpread) selection);
                    break;
            }

            // leave
            foreach (var visitor in _options.Selection)
                visitor.Leave(selection);
        }

        public void Visit(FieldSelection selection)
        {
            // enter
            foreach (var visitor in _options.FieldSelection)
                visitor.Enter(selection);

            // children
            Visit(selection.Arguments);
            Visit(selection.Directives);
            Visit(selection.SelectionSet);

            // leave
            foreach (var visitor in _options.FieldSelection)
                visitor.Leave(selection);
        }

        public void Visit(InlineFragment selection)
        {
            // enter
            foreach (var visitor in _options.InlineFragment)
                visitor.Enter(selection);

            // children
            Visit(selection.TypeCondition);
            Visit(selection.Directives);
            Visit(selection.SelectionSet);

            // leave
            foreach (var visitor in _options.InlineFragment)
                visitor.Leave(selection);
        }

        public void Visit(NamedType? namedType)
        {
            if (namedType == null)
                return;

            // enter
            foreach (var visitor in _options.NamedType)
                visitor.Enter(namedType);

            // children

            // leave
            foreach (var visitor in _options.NamedType)
                visitor.Leave(namedType);
        }

        public void Visit(FragmentSpread selection)
        {
            // enter
            foreach (var visitor in _options.FragmentSpread)
                visitor.Enter(selection);

            // children
            Visit(selection.Directives);

            // leave
            foreach (var visitor in _options.FragmentSpread)
                visitor.Leave(selection);
        }

        public void Visit(Directive directive)
        {
            // enter
            foreach (var visitor in _options.Directive)
                visitor.Enter(directive);

            // children
            Visit(directive.Arguments);

            // leave
            foreach (var visitor in _options.Directive)
                visitor.Leave(directive);
        }

        public void Visit(OperationDefinition definition)
        {
            // enter
            foreach (var visitor in _options.OperationDefinition)
                visitor.Enter(definition);

            // children
            Visit(definition.VariableDefinitions);
            Visit(definition.Directives);
            Visit(definition.SelectionSet);

            // leave
            foreach (var visitor in _options.OperationDefinition)
                visitor.Leave(definition);
        }

        public void Visit(VariableDefinition definition)
        {
            // enter
            foreach (var visitor in _options.VariableDefinition)
                visitor.Enter(definition);

            // children
            Visit(definition.Directives);
            Visit(definition.Variable);
            Visit(definition.Type);
            Visit(definition.DefaultValue);

            // leave
            foreach (var visitor in _options.VariableDefinition)
                visitor.Leave(definition);
        }

        private void Visit(IReadOnlyCollection<Argument>? arguments)
        {
            if (arguments == null)
                return;

            foreach (var argument in arguments) Visit(argument);
        }

        private void Visit(Argument argument)
        {
            // enter
            foreach (var visitor in _options.Argument)
                visitor.Enter(argument);

            // children
            Visit(argument.Value);

            // leave
            foreach (var visitor in _options.Argument)
                visitor.Leave(argument);
        }

        private void Visit(Value value)
        {
            // enter
            foreach (var visitor in _options.Value)
                visitor.Enter(value);

            // children

            // leave
            foreach (var visitor in _options.Value)
                visitor.Leave(value);
        }

        private void Visit(IReadOnlyCollection<Directive>? directives)
        {
            if (directives == null)
                return;

            foreach (var definition in directives) Visit(definition);
        }

        private void Visit(IReadOnlyCollection<VariableDefinition>? definitions)
        {
            if (definitions == null)
                return;

            foreach (var definition in definitions)
                Visit(definition);
        }

        private void Visit(DefaultValue? defaultValue)
        {
            if (defaultValue == null)
                return;

            // enter
            foreach (var visitor in _options.DefaultValue)
                visitor.Enter(defaultValue);

            // children
            Visit(defaultValue.Value);

            // leave
            foreach (var visitor in _options.DefaultValue)
                visitor.Leave(defaultValue);
        }

        private void Visit(Type type)
        {
            // enter
            foreach (var visitor in _options.Type)
                visitor.Enter(type);

            // leave
            foreach (var visitor in _options.Type)
                visitor.Leave(type);
        }
    }
}