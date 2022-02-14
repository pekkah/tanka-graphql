using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public class PrinterContext : DocumentWalkerContextBase
{
    private StringBuilder _builder { get; } = new();

    public void Append(object obj)
    {
        _builder.Append(obj);
    }

    public void AppendLine()
    {
        _builder.AppendLine();
    }

    public void AppendJoin(char separator, IEnumerable<object> items)
    {
        _builder.AppendJoin(separator, items);
    }

    public bool EndsWith(char c)
    {
        return _builder[^1] == c;
    }

    public void Rewind()
    {
        _builder.Remove(_builder.Length - 1, 1);
    }

    public override string ToString()
    {
        return _builder.ToString().Trim(' ');
    }

    public void AppendDescription(StringValue? description)
    {
        if (string.IsNullOrEmpty(description))
            return;

        var str = description.ToString();
        Append("\"\"\"");
        Append(str);
        Append("\"\"\"");
        Append(" ");
    }
}

public class Printer : ReadOnlyDocumentVisitorBase<PrinterContext>
{
    public static string Print(INode node)
    {
        var printer = new Printer();
        var context = new PrinterContext();
        var walker = new ReadOnlyDocumentWalker<PrinterContext>(
            new[] { printer },
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
            new[] { printer },
            context
        );

        walker.Visit(nodes);

        return context.ToString();
    }

    protected override void ExitValue(PrinterContext context, ValueBase value)
    {
        if (context.Parent is ICollectionNode<INode>)
            if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                context.Append(',');
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
        if (!(context.Parent is DefaultValue defaultValue)) context.Append("null");
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
        context.Append(" } ");
    }

    protected override void ExitNamedType(PrinterContext context, NamedType namedType)
    {
        context.Append(namedType.Name.Value);

        if (context.Parent is ImplementsInterfaces)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(" & ");

        if (context.Parent is UnionMemberTypes)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(" | ");
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
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(", ");
    }

    protected override void EnterArguments(PrinterContext context, Arguments arguments)
    {
        if (arguments.Count > 0) context.Append('(');
    }

    protected override void EnterArgument(PrinterContext context, Argument argument)
    {
        context.Append($"{argument.Name}: ");
    }

    protected override void ExitArgument(PrinterContext context, Argument argument)
    {
        if (context.CurrentArray?.Array is Arguments arguments)
            if (arguments.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(", ");
    }

    protected override void ExitArguments(PrinterContext context, Arguments arguments)
    {
        if (arguments.Count > 0) context.Append(')');
    }

    protected override void EnterVariable(PrinterContext context, Variable variable)
    {
        context.Append($"${variable.Name.Value}");

        if (context.Parent is VariableDefinition definition) context.Append(": ");
    }

    protected override void EnterDirective(PrinterContext context, Directive directive)
    {
        context.Append($"@{directive.Name.Value}");
    }

    protected override void ExitDirective(PrinterContext context, Directive directive)
    {
        if (context.Parent is Directives)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(' ');
    }

    protected override void EnterDefaultValue(PrinterContext context, DefaultValue defaultValue)
    {
        if (defaultValue.Value.Kind != NodeKind.NullValue)
            context.Append(" = ");
    }

    protected override void ExitVariableDefinition(PrinterContext context, VariableDefinition variableDefinition)
    {
        if (context.Parent is VariableDefinitions)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(", ");
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
        context.Append(" } ");
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
            if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                context.Append(' ');
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
            if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                context.Append(" ");
    }

    protected override void EnterDirectiveDefinition(PrinterContext context, DirectiveDefinition directiveDefinition)
    {
        context.Append(' ');
        context.AppendDescription(directiveDefinition.Description);
        context.Append("directive");
        context.Append(' ');
        context.Append($"@{directiveDefinition.Name}");

        if (directiveDefinition.IsRepeatable)
        {
            context.Append(' ');
            context.Append("repeatable");
        }
    }

    protected override void EnterArgumentsDefinition(PrinterContext context, ArgumentsDefinition argumentsDefinition)
    {
        if (argumentsDefinition.Count > 0)
            context.Append('(');
    }

    protected override void EnterInputValueDefinition(PrinterContext context, InputValueDefinition inputValueDefinition)
    {
        context.AppendDescription(inputValueDefinition.Description);
        context.Append(inputValueDefinition.Name);
        context.Append(": ");
    }

    protected override void ExitInputValueDefinition(PrinterContext context, InputValueDefinition inputValueDefinition)
    {
        if (context.Parent is ArgumentsDefinition)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(", ");

        if (context.Parent is InputFieldsDefinition)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(" ");
    }

    protected override void ExitArgumentsDefinition(PrinterContext context, ArgumentsDefinition argumentsDefinition)
    {
        if (argumentsDefinition.Count > 0)
            context.Append(")");

        if (context.Parent is FieldDefinition) context.Append(": ");
    }

    protected override void ExitDirectiveDefinition(PrinterContext context, DirectiveDefinition directiveDefinition)
    {
        context.Append(" on");
        context.Append(' ');

        var locations = directiveDefinition.DirectiveLocations;
        for (var i = 0; i < locations.Count; i++)
        {
            var location = locations[i];

            context.Append(location);

            if (i != locations.Count - 1 && location.Length > 1)
                context.Append(" | ");
        }
    }

    protected override void EnterScalarDefinition(PrinterContext context, ScalarDefinition scalarDefinition)
    {
        context.Append(' ');
        context.AppendDescription(scalarDefinition.Description);
        context.Append("scalar");
        context.Append(' ');
        context.Append(scalarDefinition.Name);
    }

    protected override void EnterFieldDefinition(PrinterContext context, FieldDefinition fieldDefinition)
    {
        context.AppendDescription(fieldDefinition.Description);
        context.Append(fieldDefinition.Name);

        if (fieldDefinition.Arguments == null || fieldDefinition.Arguments.Count == 0)
            context.Append(": ");
    }

    protected override void ExitFieldDefinition(PrinterContext context, FieldDefinition fieldDefinition)
    {
        if (context.Parent is FieldsDefinition)
            if (context.CurrentArray?.Count > 0 && !context.CurrentArray.IsLast)
                context.Append(" ");
    }

    protected override void EnterImplementsInterfaces(PrinterContext context, ImplementsInterfaces implementsInterfaces)
    {
        if (implementsInterfaces.Count == 0)
            return;

        context.Append("implements ");
    }

    protected override void EnterObjectDefinition(PrinterContext context, ObjectDefinition objectDefinition)
    {
        context.Append(' ');
        context.AppendDescription(objectDefinition.Description);
        context.Append("type ");
        context.Append(objectDefinition.Name);
        context.Append(" ");
    }

    protected override void EnterFieldsDefinition(PrinterContext context, FieldsDefinition fieldsDefinition)
    {
        context.Append(" { ");
    }

    protected override void ExitFieldsDefinition(PrinterContext context, FieldsDefinition fieldsDefinition)
    {
        context.Append(" } ");
    }

    protected override void EnterInterfaceDefinition(PrinterContext context, InterfaceDefinition interfaceDefinition)
    {
        context.Append(' ');
        context.AppendDescription(interfaceDefinition.Description);
        context.Append("interface ");
        context.Append(interfaceDefinition.Name);
        context.Append(" ");
    }

    protected override void EnterUnionDefinition(PrinterContext context, UnionDefinition unionDefinition)
    {
        context.Append(' ');
        context.AppendDescription(unionDefinition.Description);
        context.Append("union ");
        context.Append(unionDefinition.Name);
        context.Append(" ");
    }

    protected override void EnterUnionMemberTypes(PrinterContext context, UnionMemberTypes unionMemberTypes)
    {
        context.Append(" = ");
    }

    protected override void ExitUnionMemberTypes(PrinterContext context, UnionMemberTypes unionMemberTypes)
    {
        context.Append(" ");
    }

    protected override void EnterEnumDefinition(PrinterContext context, EnumDefinition enumDefinition)
    {
        context.Append(' ');
        context.AppendDescription(enumDefinition.Description);
        context.Append("enum ");
        context.Append(enumDefinition.Name);
        context.Append(" ");
    }

    protected override void EnterEnumValuesDefinition(PrinterContext context, EnumValuesDefinition enumValuesDefinition)
    {
        context.Append(" { ");
    }

    protected override void EnterEnumValueDefinition(PrinterContext context, EnumValueDefinition enumValueDefinition)
    {
        context.AppendDescription(enumValueDefinition.Description);
    }

    protected override void ExitEnumValuesDefinition(PrinterContext context, EnumValuesDefinition enumValuesDefinition)
    {
        context.Append(" } ");
    }

    protected override void ExitEnumValueDefinition(PrinterContext context, EnumValueDefinition enumValueDefinition)
    {
        context.Append(" ");
    }

    protected override void EnterInputObjectDefinition(PrinterContext context,
        InputObjectDefinition inputObjectDefinition)
    {
        context.Append(' ');
        context.AppendDescription(inputObjectDefinition.Description);
        context.Append("input ");
        context.Append(inputObjectDefinition.Name);
        context.Append(" ");
    }

    protected override void EnterInputFieldsDefinition(PrinterContext context,
        InputFieldsDefinition inputFieldsDefinition)
    {
        context.Append(" { ");
    }

    protected override void ExitInputFieldsDefinition(PrinterContext context,
        InputFieldsDefinition inputFieldsDefinition)
    {
        context.Append(" } ");
    }

    protected override void EnterSchemaDefinition(PrinterContext context, SchemaDefinition schemaDefinition)
    {
        if (schemaDefinition.Operations.Any() || schemaDefinition.Directives?.Any() == true)
        {
            context.Append(' ');
            context.AppendDescription(schemaDefinition.Description);
            context.Append("schema ");
        }
    }

    protected override void EnterRootOperationTypeDefinitions(PrinterContext context,
        RootOperationTypeDefinitions rootOperationTypeDefinitions)
    {
        if (rootOperationTypeDefinitions.Any()) context.Append(" { ");
    }

    protected override void EnterRootOperationTypeDefinition(PrinterContext context,
        RootOperationTypeDefinition rootOperationTypeDefinition)
    {
        switch (rootOperationTypeDefinition.OperationType)
        {
            case OperationType.Query:
                context.Append("query: ");
                break;
            case OperationType.Mutation:
                context.Append("mutation: ");
                break;
            case OperationType.Subscription:
                context.Append("subscription: ");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void ExitRootOperationTypeDefinition(PrinterContext context,
        RootOperationTypeDefinition rootOperationTypeDefinition)
    {
        if (context.Parent is RootOperationTypeDefinitions)
            if (context.CurrentArray?.Count > 1 && !context.CurrentArray.IsLast)
                context.Append(" ");
    }

    protected override void ExitRootOperationTypeDefinitions(PrinterContext context,
        RootOperationTypeDefinitions rootOperationTypeDefinitions)
    {
        if (rootOperationTypeDefinitions.Any()) context.Append(" } ");
    }

    protected override void EnterTypeExtension(PrinterContext context, TypeExtension typeExtension)
    {
        context.Append(" extend ");
    }
}