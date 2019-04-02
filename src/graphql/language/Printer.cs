using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.language
{
    /// <summary>
    ///     https://github.com/graphql-dotnet/parser/pull/20
    /// </summary>
    public class Printer
    {
        public string Print(ASTNode node)
        {
            if (node == null) return string.Empty;

            switch (node.Kind)
            {
                case ASTNodeKind.Document: return PrintDocument((GraphQLDocument) node);
                case ASTNodeKind.OperationDefinition:
                    return PrintOperationDefinition((GraphQLOperationDefinition) node);
                case ASTNodeKind.SelectionSet: return PrintSelectionSet((GraphQLSelectionSet) node);
                case ASTNodeKind.Field: return PrintFieldSelection((GraphQLFieldSelection) node);
                case ASTNodeKind.Name: return PrintName((GraphQLName) node);
                case ASTNodeKind.Argument: return PrintArgument((GraphQLArgument) node);
                case ASTNodeKind.FragmentSpread: return PrintFragmentSpread((GraphQLFragmentSpread) node);
                case ASTNodeKind.FragmentDefinition: return PrintFragmentDefinition((GraphQLFragmentDefinition) node);
                case ASTNodeKind.InlineFragment: return PrintInlineFragment((GraphQLInlineFragment) node);
                case ASTNodeKind.NamedType: return PrintNamedType((GraphQLNamedType) node);
                case ASTNodeKind.Directive: return PrintDirective((GraphQLDirective) node);
                case ASTNodeKind.Variable: return PrintVariable((GraphQLVariable) node);
                case ASTNodeKind.IntValue: return PrintIntValue((GraphQLScalarValue) node);
                case ASTNodeKind.FloatValue: return PrintFloatValue((GraphQLScalarValue) node);
                case ASTNodeKind.StringValue: return PrintStringValue((GraphQLScalarValue) node);
                case ASTNodeKind.BooleanValue: return PrintBooleanValue((GraphQLScalarValue) node);
                case ASTNodeKind.EnumValue: return PrintEnumValue((GraphQLScalarValue) node);
                case ASTNodeKind.ListValue: return PrintListValue((GraphQLListValue) node);
                case ASTNodeKind.ObjectValue: return PrintObjectValue((GraphQLObjectValue) node);
                case ASTNodeKind.ObjectField: return PrintObjectField((GraphQLObjectField) node);
                case ASTNodeKind.VariableDefinition: return PrintVariableDefinition((GraphQLVariableDefinition) node);
                case ASTNodeKind.NullValue: return PrintNullValue((GraphQLScalarValue) node);
                case ASTNodeKind.SchemaDefinition: return PrintSchemaDefinition((GraphQLSchemaDefinition) node);
                case ASTNodeKind.ListType: return PrintListType((GraphQLListType) node);
                case ASTNodeKind.NonNullType: return PrintNonNullType((GraphQLNonNullType) node);
                case ASTNodeKind.OperationTypeDefinition:
                    return PrintOperationTypeDefinition((GraphQLOperationTypeDefinition) node);
                case ASTNodeKind.ScalarTypeDefinition:
                    return PrintScalarTypeDefinition((GraphQLScalarTypeDefinition) node);
                case ASTNodeKind.ObjectTypeDefinition:
                    return PrintObjectTypeDefinition((GraphQLObjectTypeDefinition) node);
                case ASTNodeKind.FieldDefinition: return PrintFieldDefinition((GraphQLFieldDefinition) node);
                case ASTNodeKind.InputValueDefinition:
                    return PrintInputValueDefinition((GraphQLInputValueDefinition) node);
                case ASTNodeKind.InterfaceTypeDefinition:
                    return PrintInterfaceTypeDefinition((GraphQLInterfaceTypeDefinition) node);
                case ASTNodeKind.UnionTypeDefinition:
                    return PrintUnionTypeDefinition((GraphQLUnionTypeDefinition) node);
                case ASTNodeKind.EnumTypeDefinition: return PrintEnumTypeDefinition((GraphQLEnumTypeDefinition) node);
                case ASTNodeKind.EnumValueDefinition:
                    return PrintEnumValueDefinition((GraphQLEnumValueDefinition) node);
                case ASTNodeKind.InputObjectTypeDefinition:
                    return PrintInputObjectTypeDefinition((GraphQLInputObjectTypeDefinition) node);
                case ASTNodeKind.TypeExtensionDefinition:
                    return PrintTypeExtensionDefinition((GraphQLTypeExtensionDefinition) node);
                case ASTNodeKind.DirectiveDefinition:
                    return PrintDirectiveDefinition((GraphQLDirectiveDefinition) node);
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

        private string PrintArgument(GraphQLArgument argument)
        {
            var name = PrintName(argument.Name);
            var value = Print(argument.Value);

            return $"{name}: {value}";
        }

        private string PrintBooleanValue(GraphQLScalarValue node)
        {
            return node.Value;
        }

        private string PrintDirective(GraphQLDirective directive)
        {
            var name = PrintName(directive.Name);
            var args = directive.Arguments?.Select(PrintArgument);

            return $"@{name}{Wrap("(", Join(args, ", "), ")")}";
        }

        private string PrintDirectiveDefinition(GraphQLDirectiveDefinition node)
        {
            var name = PrintName(node.Name);
            var args = node.Arguments?.Select(Print);
            var locations = node.Locations?.Select(PrintName);

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

        private string PrintDocument(GraphQLDocument node)
        {
            var definitions = node.Definitions?.Select(Print);

            return Join(definitions, $"{Environment.NewLine}{Environment.NewLine}");
        }

        private string PrintEnumTypeDefinition(GraphQLEnumTypeDefinition node)
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

        private string PrintEnumValue(GraphQLScalarValue node)
        {
            return node.Value;
        }

        private string PrintEnumValueDefinition(GraphQLEnumValueDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);

            return Join(new[]
                {
                    name,
                    Join(directives, " ")
                },
                " ");
        }

        private string PrintFieldDefinition(GraphQLFieldDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);
            var args = node.Arguments?.Select(PrintInputValueDefinition);
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

        private string PrintFieldSelection(GraphQLFieldSelection node)
        {
            var alias = PrintName(node.Alias);
            var name = PrintName(node.Name);
            var args = node.Arguments?.Select(PrintArgument);
            var directives = node.Directives?.Select(PrintDirective);
            var selectionSet = PrintSelectionSet(node.SelectionSet);

            return Join(new[]
            {
                $"{Wrap(string.Empty, alias, ": ")}{name}{Wrap("(", Join(args, ", "), ")")}",
                Join(directives, " "),
                selectionSet
            });
        }

        private string PrintFloatValue(GraphQLScalarValue node)
        {
            return node.Value;
        }

        private string PrintFragmentDefinition(GraphQLFragmentDefinition node)
        {
            var name = PrintName(node.Name);
            var typeCondition = PrintNamedType(node.TypeCondition);
            var directives = node.Directives?.Select(PrintDirective);
            var selectionSet = PrintSelectionSet(node.SelectionSet);

            return $"fragment {name} on {typeCondition} {Wrap(string.Empty, Join(directives, " "), " ")}{selectionSet}";
        }

        private string PrintFragmentSpread(GraphQLFragmentSpread node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);

            return $"...{name}{Wrap(string.Empty, Join(directives, " "))}";
        }

        private string PrintInlineFragment(GraphQLInlineFragment node)
        {
            var typeCondition = PrintNamedType(node.TypeCondition);
            var directives = node.Directives?.Select(PrintDirective);
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

        private string PrintInputObjectTypeDefinition(GraphQLInputObjectTypeDefinition node)
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

        private string PrintInputValueDefinition(GraphQLInputValueDefinition node)
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

        private string PrintInterfaceTypeDefinition(GraphQLInterfaceTypeDefinition node)
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

        private string PrintIntValue(GraphQLScalarValue node)
        {
            return node.Value;
        }

        private string PrintListType(GraphQLListType node)
        {
            var type = Print(node.Type);

            return $"[{type}]";
        }

        private string PrintListValue(GraphQLListValue node)
        {
            var values = node.Values?.Select(Print);

            return $"[{Join(values, ", ")}]";
        }

        private string PrintName(GraphQLName name)
        {
            return name?.Value ?? string.Empty;
        }

        private string PrintNamedType(GraphQLNamedType node)
        {
            if (node == null) return string.Empty;

            return PrintName(node.Name);
        }

        private string PrintNonNullType(GraphQLNonNullType node)
        {
            var type = Print(node.Type);

            return $"{type}!";
        }

        private string PrintNullValue(GraphQLScalarValue node)
        {
            return "null";
        }

        private string PrintObjectField(GraphQLObjectField node)
        {
            var name = PrintName(node.Name);
            var value = Print(node.Value);

            return $"{name}: {value}";
        }

        private string PrintObjectTypeDefinition(GraphQLObjectTypeDefinition node)
        {
            var name = PrintName(node.Name);
            var interfaces = node.Interfaces?.Select(PrintNamedType);
            var directives = node.Directives?.Select(PrintDirective);
            var fields = node.Fields?.Select(PrintFieldDefinition);

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

        private string PrintObjectValue(GraphQLObjectValue node)
        {
            var fields = node.Fields?.Select(PrintObjectField);

            return "{" + Join(fields, ", ") + "}";
        }

        private string PrintOperationDefinition(GraphQLOperationDefinition definition)
        {
            var name = PrintName(definition.Name);
            var directives = Join(definition.Directives?.Select(PrintDirective), " ");
            var selectionSet = PrintSelectionSet(definition.SelectionSet);

            var variableDefinitions = Wrap(
                "(",
                Join(definition.VariableDefinitions?.Select(PrintVariableDefinition), ", "), ")");

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

        private string PrintOperationType(GraphQLOperationTypeDefinition operationType)
        {
            var operation = operationType.Operation.ToString().ToLower();
            var type = PrintNamedType(operationType.Type);

            return $"{operation}: {type}";
        }

        private string PrintOperationTypeDefinition(GraphQLOperationTypeDefinition node)
        {
            var operation = node.Operation.ToString();
            var type = PrintNamedType(node.Type);

            return $"{operation}: {type}";
        }

        private string PrintScalarTypeDefinition(GraphQLScalarTypeDefinition node)
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

        private string PrintSchemaDefinition(GraphQLSchemaDefinition node)
        {
            var directives = node.Directives?.Select(PrintDirective);
            var operationTypes = node.OperationTypes?.Select(PrintOperationType);

            return Join(new[]
                {
                    "schema",
                    Join(directives, " "),
                    Block(operationTypes) ?? "{ }"
                },
                " ");
        }

        private string PrintSelectionSet(GraphQLSelectionSet selectionSet)
        {
            if (selectionSet == null) return string.Empty;

            return Block(selectionSet.Selections?.Select(Print));
        }

        private string PrintStringValue(GraphQLScalarValue node)
        {
            return $"\"{node.Value}\"";
        }

        private string PrintTypeExtensionDefinition(GraphQLTypeExtensionDefinition node)
        {
            return $"extend {Print(node.Definition)}";
        }

        private string PrintUnionTypeDefinition(GraphQLUnionTypeDefinition node)
        {
            var name = PrintName(node.Name);
            var directives = node.Directives?.Select(PrintDirective);
            var types = node.Types?.Select(PrintNamedType);

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

        private string PrintVariable(GraphQLVariable variable)
        {
            return $"${variable.Name.Value}";
        }

        private string PrintVariableDefinition(GraphQLVariableDefinition variableDefinition)
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
                ? null
                : $"{start}{maybeString}{end}";
        }
    }
}