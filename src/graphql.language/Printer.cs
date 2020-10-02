using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public class PrinterContext: DocumentWalkerContextBase
    {
        private StringBuilder _builder { get; } = new StringBuilder();
        
        public void Append(object obj) => _builder.Append(obj);

        public void AppendLine() => _builder.AppendLine();

        public void AppendJoin(char separator, IEnumerable<object> items) => _builder.AppendJoin(separator, items);

        public bool EndsWith(char c)
        {
            return _builder[^1] == c;
        }

        public void Rewind()
        {
            _builder.Remove(_builder.Length-1, 1);
        }
        
        public override string ToString()
        {
            return _builder.ToString();
        }
    }

    public class Printer : ReadOnlyDocumentVisitorBase<PrinterContext>
    {
        public static string Print(INode node)
        {
            var printer = new Printer();
            var context = new PrinterContext();
            var walker = new ReadOnlyDocumentWalker<PrinterContext>(
                new[] {printer},
                context
            );

            walker.Visit(node);
            return context.ToString();
        }

        public static string Print(ICollectionNode<INode> nodes)
        {
            var printer = new Printer();
            var context = new PrinterContext();
            var walker = new ReadOnlyDocumentWalker<PrinterContext>(
                new[] {printer},
                context
            );

            walker.Visit(nodes);

            return context.ToString();
        }

        protected override void ExitValue(PrinterContext context, ValueBase value)
        {
            if (context.Parent is ICollectionNode<INode>)
            {
                if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                    context.Append(',');
            }
        }

        protected override void ExitIntValue(PrinterContext context, IntValue intValue)
        {
            context.Append(intValue.Value);
        }

        protected override void ExitFloatValue(PrinterContext context, FloatValue floatValue)
        {
            var str = Encoding.UTF8.GetString(floatValue.ValueSpan);
            context.Append(str);
        }

        protected override void ExitEnumValue(PrinterContext context, EnumValue enumValue)
        {
            context.Append(enumValue.Name.Value);
        }

        protected override void ExitBooleanValue(PrinterContext context, BooleanValue booleanValue)
        {
            context.Append(booleanValue.Value.ToString().ToLowerInvariant());
        }

        protected override void ExitStringValue(PrinterContext context, StringValue stringValue)
        {
            if (stringValue.ValueSpan.IndexOf((byte)'\n') != -1)
            {
                context.Append("\"\"\"");
                context.Append(Encoding.UTF8.GetString(stringValue.ValueSpan));
                context.Append("\"\"\"");
            }
            else
            {
                context.Append("\"");
                context.Append(Encoding.UTF8.GetString(stringValue.ValueSpan));
                context.Append("\"");
            }
        }

        protected override void ExitNullValue(PrinterContext context, NullValue nullValue)
        {
            context.Append("null");
        }

        protected override void EnterListValue(PrinterContext context, ListValue listValue)
        {
            context.Append("[");
        }

        protected override void ExitListValue(PrinterContext context, ListValue listValue)
        {
            context.Append("]");
        }

        protected override void EnterObjectValue(PrinterContext context, ObjectValue objectValue)
        {
            context.Append("{ ");
        }

        protected override void ExitObjectValue(PrinterContext context, ObjectValue objectValue)
        {
            context.Append(" }");
        }

        protected override void ExitNamedType(PrinterContext context, NamedType namedType)
        {
            context.Append(namedType.Name.Value);
        }

        protected override void EnterListType(PrinterContext context, ListType listType)
        {
            context.Append("[");
        }
        
        protected override void ExitListType(PrinterContext context, ListType listType)
        {
            context.Append("]");
        }

        protected override void ExitNonNullType(PrinterContext context, NonNullType nonNullType)
        {
            context.Append("!");
        }

        protected override void EnterObjectField(PrinterContext context, ObjectField objectField)
        {
            context.Append($"{objectField.Name.Value}: ");
        }

        protected override void ExitObjectField(PrinterContext context, ObjectField objectField)
        {
            if (context.Parent is ObjectValue)
            {
                if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                    context.Append(", ");
            }
        }

        protected override void EnterArguments(PrinterContext context, Arguments arguments)
        {
            context.Append('(');
        }

        protected override void EnterArgument(PrinterContext context, Argument argument)
        {
            context.Append($"{argument.Name}: ");
        }

        protected override void ExitArgument(PrinterContext context, Argument argument)
        {
            if (context.CurrentArray?.Array is Arguments arguments)
            {
                if (arguments.Count > 1 && !context.CurrentArray.IsLast)
                    context.Append(", ");
            }
        }

        protected override void ExitArguments(PrinterContext context, Arguments arguments)
        {
            context.Append(')');
        }

        protected override void EnterVariable(PrinterContext context, Variable variable)
        {
            context.Append($"${variable.Name.Value}");

            if (context.Parent is VariableDefinition definition)
            {
                context.Append(": ");
            }
        }

        protected override void EnterDirective(PrinterContext context, Directive directive)
        {
            context.Append($"@{directive.Name.Value}");
        }

        protected override void ExitDirective(PrinterContext context, Directive directive)
        {
            if (context.Parent is Directives)
            {
                if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                    context.Append(' ');
            }
        }

        protected override void EnterDefaultValue(PrinterContext context, DefaultValue defaultValue)
        {
            context.Append(" = ");
        }

        protected override void ExitVariableDefinition(PrinterContext context, VariableDefinition variableDefinition)
        {
            if (context.Parent is VariableDefinitions)
            {
                if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                    context.Append(", ");
            }
        }

        protected override void EnterVariableDefinitions(PrinterContext context, VariableDefinitions variableDefinition)
        {
            context.Append('(');
        }

        protected override void ExitVariableDefinitions(PrinterContext context, VariableDefinitions variableDefinition)
        {
            context.Append(')');
        }

        protected override void EnterInlineFragment(PrinterContext context, InlineFragment inlineFragment)
        {
            context.Append("... ");

            if (inlineFragment.TypeCondition != null)
                context.Append("on ");
        }

        protected override void EnterSelectionSet(PrinterContext context, SelectionSet selectionSet)
        {
            context.Append(" { ");
        }

        protected override void ExitSelectionSet(PrinterContext context, SelectionSet selectionSet)
        {
            context.Append(" }");
        }

        protected override void EnterDirectives(PrinterContext context, Directives directives)
        {
            if (context.Parent != null)
                context.Append(' ');
        }

        protected override void EnterFieldSelection(PrinterContext context, FieldSelection fieldSelection)
        {
            if (fieldSelection.Alias != default)
            {
                context.Append(fieldSelection.Alias);
                context.Append(": ");
            }

            context.Append($"{fieldSelection.Name}");

            if (context.Parent is SelectionSet)
            {
                if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                    context.Append(' ');
            }
        }

        protected override void EnterFragmentDefinition(PrinterContext context, FragmentDefinition fragmentDefinition)
        {
            context.Append("fragment");
            context.Append(' ');
            context.Append(fragmentDefinition.FragmentName);

            context.Append(' ');
            context.Append("on");
            context.Append(' ');
        }

        protected override void EnterFragmentSpread(PrinterContext context, FragmentSpread fragmentSpread)
        {
            context.Append("...");
            context.Append(fragmentSpread.FragmentName);
        }

        protected override void EnterOperationDefinition(PrinterContext context, OperationDefinition operationDefinition)
        {
            if (!operationDefinition.IsShort)
                context.Append(operationDefinition.Operation.ToString().ToLowerInvariant());
        }

        protected override void ExitOperationDefinition(PrinterContext context, OperationDefinition operationDefinition)
        {
            if (context.Parent is ICollectionNode<OperationDefinition>)
            {
                if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                    context.Append(" ");
            }
        }
    }
}