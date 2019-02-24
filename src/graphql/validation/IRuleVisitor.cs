using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface IRuleVisitor
    {
        NodeVisitor<GraphQLName> EnterAlias { get; set; }
        NodeVisitor<GraphQLArgument> EnterArgument { get; set; }
        NodeVisitor<GraphQLScalarValue> EnterBooleanValue { get; set; }
        NodeVisitor<GraphQLDirective> EnterDirective { get; set; }
        NodeVisitor<GraphQLDirective> EnterDirectives { get; set; }
        NodeVisitor<GraphQLDocument> EnterDocument { get; set; }
        NodeVisitor<GraphQLScalarValue> EnterEnumValue { get; set; }
        NodeVisitor<GraphQLFieldSelection> EnterFieldSelection { get; set; }
        NodeVisitor<GraphQLScalarValue> EnterFloatValue { get; set; }
        NodeVisitor<GraphQLFragmentDefinition> EnterFragmentDefinition { get; set; }
        NodeVisitor<GraphQLFragmentSpread> EnterFragmentSpread { get; set; }
        NodeVisitor<GraphQLInlineFragment> EnterInlineFragment { get; set; }
        NodeVisitor<GraphQLScalarValue> EnterIntValue { get; set; }
        NodeVisitor<GraphQLListValue> EnterListValue { get; set; }
        NodeVisitor<GraphQLName> EnterName { get; set; }
        NodeVisitor<GraphQLNamedType> EnterNamedType { get; set; }
        NodeVisitor<ASTNode> EnterNode { get; set; }
        NodeVisitor<GraphQLObjectField> EnterObjectField { get; set; }
        NodeVisitor<GraphQLObjectValue> EnterObjectValue { get; set; }
        NodeVisitor<GraphQLOperationDefinition> EnterOperationDefinition { get; set; }
        NodeVisitor<GraphQLSelectionSet> EnterSelectionSet { get; set; }
        NodeVisitor<GraphQLScalarValue> EnterStringValue { get; set; }
        NodeVisitor<GraphQLVariable> EnterVariable { get; set; }
        NodeVisitor<GraphQLVariableDefinition> EnterVariableDefinition { get; set; }
        NodeVisitor<GraphQLArgument> LeaveArgument { get; set; }
        NodeVisitor<GraphQLDirective> LeaveDirective { get; set; }
        NodeVisitor<GraphQLDocument> LeaveDocument { get; set; }
        NodeVisitor<GraphQLScalarValue> LeaveEnumValue { get; set; }
        NodeVisitor<GraphQLFieldSelection> LeaveFieldSelection { get; set; }
        NodeVisitor<GraphQLFragmentDefinition> LeaveFragmentDefinition { get; set; }
        NodeVisitor<GraphQLInlineFragment> LeaveInlineFragment { get; set; }
        NodeVisitor<GraphQLListValue> LeaveListValue { get; set; }
        NodeVisitor<GraphQLObjectField> LeaveObjectField { get; set; }
        NodeVisitor<GraphQLObjectValue> LeaveObjectValue { get; set; }
        NodeVisitor<GraphQLOperationDefinition> LeaveOperationDefinition { get; set; }
        NodeVisitor<GraphQLSelectionSet> LeaveSelectionSet { get; set; }
        NodeVisitor<GraphQLVariable> LeaveVariable { get; set; }
        NodeVisitor<GraphQLVariableDefinition> LeaveVariableDefinition { get; set; }
    }
}