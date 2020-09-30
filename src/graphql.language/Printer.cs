using System;
using System.Collections.Generic;
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

        public static string Print(IReadOnlyCollection<INode> nodes)
        {
            var printer = new Printer();
            var context = new PrinterContext();
            var walker = new ReadOnlyDocumentWalker<PrinterContext>(
                new[] {printer},
                context
            );

            walker.VisitCollection(nodes);

            return context.ToString();
        }

        public override void ExitNode(PrinterContext context, INode node)
        {
            base.ExitNode(context, node);
            
            if (node is Value && node != context.Parent && node.Kind != NodeKind.ObjectValue)
                if (context.Parent?.Kind == NodeKind.ListValue)
                    context.Append(",");
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
            if (context.EndsWith(','))
                context.Rewind();
            
            context.Append("]");
        }

        protected override void EnterObjectValue(PrinterContext context, ObjectValue objectValue)
        {
            context.Append("{");
        }

        protected override void ExitObjectValue(PrinterContext context, ObjectValue objectValue)
        {
            if (context.EndsWith(','))
                context.Rewind();
            
            context.Append("}");
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
            context.Append(",");
        }

        protected override void EnterArguments(PrinterContext context, IReadOnlyCollectionNode<Argument> arguments)
        {
            context.Append('(');
        }

        protected override void EnterArgument(PrinterContext context, Argument argument)
        {
            context.Append($"{argument.Name}:");
        }

        protected override void ExitArguments(PrinterContext context, ReadOnlyCollectionNode<Argument> arguments)
        {
            if(context.EndsWith(','))
                context.Rewind();
            
            context.Append(')');
        }
    }
}