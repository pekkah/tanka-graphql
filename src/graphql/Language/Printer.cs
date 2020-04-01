using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;


namespace Tanka.GraphQL.Language
{
    /// <summary>
    ///     https://github.com/graphql-dotnet/parser/pull/20
    /// </summary>
    public class Printer
    {
        public string Print(INode node)
        {
            if (node == null) return string.Empty;

            switch (node.Kind)
            {
                case NodeKind.ExecutableDocument: return PrintDocument((ExecutableDocument) node);
                case NodeKind.OperationDefinition:
                    return PrintOperationDefinition((OperationDefinition) node);
                case NodeKind.SelectionSet: return PrintSelectionSet((SelectionSet) node);
                case NodeKind.FieldSelection: return PrintFieldSelection((FieldSelection) node);
                case NodeKind.Argument: return PrintArgument((Argument) node);
                case NodeKind.FragmentSpread: return PrintFragmentSpread((FragmentSpread) node);
                case NodeKind.FragmentDefinition: return PrintFragmentDefinition((FragmentDefinition) node);
                case NodeKind.InlineFragment: return PrintInlineFragment((InlineFragment) node);
                case NodeKind.NamedType: return PrintNamedType((NamedType) node);
                case NodeKind.Directive: return PrintDirective((Directive) node);
                case NodeKind.Variable: return PrintVariable((Variable) node);
                case NodeKind.IntValue: return PrintIntValue((IntValue) node);
                case NodeKind.FloatValue: return PrintFloatValue((FloatValue) node);
                case NodeKind.StringValue: return PrintStringValue((StringValue) node);
                case NodeKind.BooleanValue: return PrintBooleanValue((BooleanValue) node);
                case NodeKind.EnumValue: return PrintEnumValue((EnumValue) node);
                case NodeKind.ListValue: return PrintListValue((ListValue) node);
                case NodeKind.ObjectValue: return PrintObjectValue((ObjectValue) node);
                case NodeKind.ObjectField: return PrintObjectField((ObjectField) node);
                case NodeKind.VariableDefinition: return PrintVariableDefinition((VariableDefinition) node);
                case NodeKind.NullValue: return PrintNullValue((NullValue) node);
                case NodeKind.SchemaDefinition: return PrintSchemaDefinition((SchemaDefinition) node);
                case NodeKind.ListType: return PrintListType((ListType) node);
                case NodeKind.NonNullType: return PrintNonNullType((NonNullType) node);
                case NodeKind.ScalarDefinition:
                    return PrintScalarTypeDefinition((ScalarDefinition) node);
                case NodeKind.ObjectDefinition:
                    return PrintObjectTypeDefinition((ObjectDefinition) node);
                case NodeKind.FieldDefinition: return PrintFieldDefinition((FieldDefinition) node);
                case NodeKind.InputValueDefinition:
                    return PrintInputValueDefinition((InputValueDefinition) node);
                case NodeKind.InterfaceDefinition:
                    return PrintInterfaceTypeDefinition((InterfaceDefinition) node);
                case NodeKind.UnionDefinition:
                    return PrintUnionTypeDefinition((UnionDefinition) node);
                case NodeKind.EnumDefinition: return PrintEnumTypeDefinition((EnumDefinition) node);
                case NodeKind.EnumValueDefinition:
                    return PrintEnumValueDefinition((EnumValueDefinition) node);
                case NodeKind.InputObjectDefinition:
                    return PrintInputObjectTypeDefinition((InputObjectDefinition) node);
                case NodeKind.TypeExtension:
                    return PrintTypeExtensionDefinition((TypeDefinition) node);
                case NodeKind.DirectiveDefinition:
                    return PrintDirectiveDefinition((DirectiveDefinition) node);
            }

            return string.Empty;
        }

        private string Block(IEnumerable<string> enumerable)
        {
            return enumerable?.Any() == true
                ? "{" + Environment.NewLine + Indent(Join(enumerable, Environment.NewLine)) + Environment.NewLine + "}"
                : null;
        }

        private string Indent(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? null
                : $"  {input.Replace(Environment.NewLine, $"{Environment.NewLine}  ")}";
        }

        private string Join(IEnumerable<string> collection, string separator = "")
        {
            collection = collection?.Where(e => !string.IsNullOrWhiteSpace(e));

            return collection?.Any() == true
                ? string.Join(separator, collection)
                : string.Empty;
        }

        private string PrintArgument(Argument argument)
        {
            var name = PrintName(argument.Name);
            var value = Print(argument.Value);

            return $"{name}: {value}";
        }

        private string PrintBooleanValue(BooleanValue node)
        {
            return node.Value.ToString(CultureInfo.InvariantCulture);
        }

        private string PrintDirective(Directive directive)
        {
            var name = PrintName(directive.Name);
            var args = directive.Arguments?.Select(PrintArgument);

            return $"@{name}{Wrap("(", Join(args, ", "), ")")}";
        }

        private string PrintDirectiveDefinition(DirectiveDefinition node)
        {
            var name = PrintName(node.Name);
            var args = node.Arguments?.Select(Print);
            var locations = node.DirectiveLocations;

            return Join(new[]
            {
                "directive @",
                name,
                args.All(e => !e.Contains(Environment.NewLine))
                    ? Wrap("(", Join(args, ", "), ")")
                    : Wrap(
                        $"({Environment.NewLine}",
                        Indent(Join(args, Environment.NewLine)),
                        $"{Environment.NewLine})"),
                " on ",
                Join(locations, " | ")
            });
        }

        private string PrintDocument(ExecutableDocument node)
        {
            var nodes = new List<INode>();
            if (node.OperationDefinitions != null)
                nodes.AddRange(node.OperationDefinitions);
            
            if (node.FragmentDefinitions != null)
                nodes.AddRange(node.FragmentDefinitions);

            var definitions = nodes.Select(Print);
            return Join(definitions, $"{Environment.NewLine}");
        }

        private string PrintEnumTypeDefinition(EnumDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);
            var values = node.Values?.Select(PrintEnumValueDefinition);

            return Join(new[]
                {
                    "enum",
                    name,
                    Join(directives, " "),
                    Block(values) ?? "{ }"
                },
                " ");
        }

        private string PrintEnumValue(EnumValue node)
        {
            return node.Name;
        }

        private string PrintEnumValueDefinition(EnumValueDefinition node)
        {
            var name = PrintName(node.Value.Name);
            var directives = node.Directives?
                .Select(PrintDirective) ?? Array.Empty<string>();

            return Join(new[]
                {
                    name,
                    Join(directives, " ")
                },
                " ");
        }

        private string PrintFieldDefinition(FieldDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?
                .Select(PrintDirective) ?? Array.Empty<string>();
            
            var args = node.Arguments?
                .Select(PrintInputValueDefinition) ?? Array.Empty<string>();
            
            var type = Print(node.Type);

            return Join(new[]
            {
                name,
                args.All(e => !e.Contains(Environment.NewLine))
                    ? Wrap("(", Join(args, ", "), ")")
                    : Wrap(
                        $"({Environment.NewLine}",
                        Indent(Join(args, Environment.NewLine)),
                        $"{Environment.NewLine}aaa)"),
                ": ",
                type,
                Wrap(" ", Join(directives, " "))
            });
        }

        private string PrintFieldSelection(FieldSelection node)
        {
            var alias = node.Alias != null ? PrintName(node.Alias.Value): string.Empty;
            var name = PrintName(node.Name);
            var args = node.Arguments?.Select(PrintArgument) ?? Array.Empty<string>();
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var selectionSet = node.SelectionSet != null ? PrintSelectionSet(node.SelectionSet): string.Empty;

            return Join(new[]
            {
                $"{Wrap(string.Empty, alias, ": ")}{name}{Wrap("(", Join(args, ", "), ")")}",
                Join(directives, " "),
                selectionSet
            });
        }

        private string PrintFloatValue(FloatValue node)
        {
            return Encoding.UTF8.GetString(node.ValueSpan);
        }

        private string PrintFragmentDefinition(FragmentDefinition node)
        {
            var name = PrintName(node.FragmentName);
            var typeCondition = PrintNamedType(node.TypeCondition);
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var selectionSet = PrintSelectionSet(node.SelectionSet);

            return $"fragment {name} on {typeCondition} {Wrap(string.Empty, Join(directives, " "), " ")}{selectionSet}";
        }

        private string PrintFragmentSpread(FragmentSpread node)
        {
            var name = PrintName(node.FragmentName);
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();

            return $"...{name}{Wrap(string.Empty, Join(directives, " "))}";
        }

        private string PrintInlineFragment(InlineFragment node)
        {
            var typeCondition = node.TypeCondition != null ? PrintNamedType(node.TypeCondition): string.Empty;
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var selectionSet = PrintSelectionSet(node.SelectionSet);

            return Join(new[]
                {
                    "...",
                    Wrap("on ", typeCondition),
                    Join(directives, " "),
                    selectionSet
                },
                " ");
        }

        private string PrintInputObjectTypeDefinition(InputObjectDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);
            var fields = node.Fields?.Select(PrintInputValueDefinition);

            return Join(new[]
                {
                    "input",
                    name,
                    Join(directives, " "),
                    Block(fields) ?? "{ }"
                },
                " ");
        }

        private string PrintInputValueDefinition(InputValueDefinition node)
        {
            var name = PrintName(node.Name);
            var type = Print(node.Type);
            var directives = node.Directives?.Select(PrintDirective);
            var defaultValue = Print(node.DefaultValue);

            return Join(new[]
                {
                    $"{name}: {type}",
                    Wrap("= ", defaultValue),
                    Join(directives, " ")
                },
                " ");
        }

        private string PrintInterfaceTypeDefinition(InterfaceDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);
            var fields = node.Fields?.Select(PrintFieldDefinition);

            return Join(new[]
                {
                    "interface",
                    name,
                    Join(directives, " "),
                    Block(fields) ?? "{ }"
                },
                " ");
        }

        private string PrintIntValue(IntValue node)
        {
            return node.Value.ToString(NumberFormatInfo.InvariantInfo);
;        }

        private string PrintListType(ListType node)
        {
            var type = Print(node.OfType);

            return $"[{type}]";
        }

        private string PrintListValue(ListValue node)
        {
            var values = node.Values?.Select(Print);

            return $"[{Join(values, ", ")}]";
        }

        private string PrintName(Name? name)
        {
            if (name == null)
                return string.Empty;
            
            return name.Value.AsString();
        }

        private string PrintNamedType(NamedType node)
        {
            if (node == null) return string.Empty;

            return PrintName(node.Name);
        }

        private string PrintNonNullType(NonNullType node)
        {
            var type = Print(node.OfType);

            return $"{type}!";
        }

        private string PrintNullValue(NullValue node)
        {
            return "null";
        }

        private string PrintObjectField(ObjectField node)
        {
            var name = PrintName(node.Name);
            var value = Print(node.Value);

            return $"{name}: {value}";
        }

        private string PrintObjectTypeDefinition(ObjectDefinition node)
        {
            var name = PrintName(node.Name);
            var interfaces = node.Interfaces?.Select(PrintNamedType) ?? Array.Empty<string>();
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var fields = node.Fields?.Select(PrintFieldDefinition) ?? Array.Empty<string>();

            return Join(new[]
                {
                    "type",
                    name,
                    Wrap("implements ", Join(interfaces, " & ")),
                    Join(directives, " "),
                    Block(fields) ?? "{ }"
                },
                " ");
        }

        private string PrintObjectValue(ObjectValue node)
        {
            var fields = node.Fields?.Select(PrintObjectField) ?? Array.Empty<string>();

            return "{" + Join(fields, ", ") + "}";
        }

        private string PrintOperationDefinition(OperationDefinition definition)
        {
            var name = PrintName(definition.Name);
            var directives = Join(definition.Directives?.Select(PrintDirective) ?? Array.Empty<string>(), " ");
            var selectionSet = PrintSelectionSet(definition.SelectionSet);

            var variableDefinitions = Wrap(
                "(",
                Join(definition.VariableDefinitions?.Select(PrintVariableDefinition) ?? Array.Empty<string>(), ", "), ")");

            var operation = definition.Operation
                .ToString()
                .ToLower();

            return string.IsNullOrWhiteSpace(name) &&
                   string.IsNullOrWhiteSpace(name) &&
                   string.IsNullOrWhiteSpace(name) &&
                   definition.Operation == OperationType.Query
                ? selectionSet
                : Join(
                    new[]
                    {
                        operation,
                        Join(new[] {name, variableDefinitions}),
                        directives,
                        selectionSet
                    },
                    " ");
        }

        private string PrintOperationType(OperationType operationType, NamedType namedType)
        {
            var operation = operationType.ToString().ToLower();
            var type = PrintNamedType(namedType);

            return $"{operation}: {type}";
        }
        

        private string PrintScalarTypeDefinition(ScalarDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);

            return Join(new[]
                {
                    "scalar",
                    name,
                    Join(directives, " ")
                },
                " ");
        }

        private string PrintSchemaDefinition(SchemaDefinition node)
        {
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var operationTypes = node.Operations
                .Select(op => PrintOperationType(op.Operation, op.NamedType));

            return Join(new[]
                {
                    "schema",
                    Join(directives, " "),
                    Block(operationTypes) ?? "{ }"
                },
                " ");
        }

        private string PrintSelectionSet(SelectionSet selectionSet)
        {
            if (selectionSet == null) return string.Empty;

            return Block(selectionSet.Selections?.Select(Print) ?? Array.Empty<string>());
        }

        private string PrintStringValue(StringValue node)
        {
            return $"\"{node}\"";
        }

        private string PrintTypeExtensionDefinition(TypeDefinition node)
        {
            return $"extend {Print(node)}";
        }

        private string PrintUnionTypeDefinition(UnionDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective) ?? Array.Empty<string>();
            var types = node.Members?.Select(PrintNamedType).ToArray() ?? Array.Empty<string>();

            return Join(new[]
                {
                    "union",
                    name,
                    Join(directives, " "),
                    types?.Any() == true
                        ? "= " + Join(types, " | ")
                        : string.Empty
                },
                " ");
        }

        private string PrintVariable(Variable variable)
        {
            return $"${variable.Name.Value}";
        }

        private string PrintVariableDefinition(VariableDefinition variableDefinition)
        {
            var variable = PrintVariable(variableDefinition.Variable);
            var type = Print(variableDefinition.Type);
            var defaultValue = variableDefinition.DefaultValue?.ToString();

            return Join(new[]
            {
                variable,
                ": ",
                type,
                Wrap(" = ", defaultValue)
            });
        }

        private string Wrap(string start, string maybeString, string end = "")
        {
            return string.IsNullOrWhiteSpace(maybeString)
                ? string.Empty
                : $"{start}{maybeString}{end}";
        }
    }
}