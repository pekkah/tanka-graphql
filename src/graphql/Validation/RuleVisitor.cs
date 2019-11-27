using GraphQLParser.AST;

namespace Tanka.GraphQL.Validation
{
    public class RuleVisitor
    {
        public NodeVisitor<GraphQLName> EnterAlias { get; set; }

        public NodeVisitor<GraphQLArgument> EnterArgument { get; set; }

        public NodeVisitor<GraphQLScalarValue> EnterBooleanValue { get; set; }

        public NodeVisitor<GraphQLDirective> EnterDirective { get; set; }

        public NodeVisitor<GraphQLDirective> EnterDirectives { get; set; }

        public NodeVisitor<GraphQLDocument> EnterDocument { get; set; }

        public NodeVisitor<GraphQLScalarValue> EnterEnumValue { get; set; }

        public NodeVisitor<GraphQLFieldSelection> EnterFieldSelection { get; set; }

        public NodeVisitor<GraphQLScalarValue> EnterFloatValue { get; set; }

        public NodeVisitor<GraphQLFragmentDefinition> EnterFragmentDefinition { get; set; }

        public NodeVisitor<GraphQLFragmentSpread> EnterFragmentSpread { get; set; }

        public NodeVisitor<GraphQLInlineFragment> EnterInlineFragment { get; set; }

        public NodeVisitor<GraphQLScalarValue> EnterIntValue { get; set; }

        public NodeVisitor<GraphQLListValue> EnterListValue { get; set; }

        public NodeVisitor<GraphQLName> EnterName { get; set; }

        public NodeVisitor<GraphQLNamedType> EnterNamedType{ get; set; }

        public NodeVisitor<ASTNode> EnterNode{ get; set; }

        public NodeVisitor<GraphQLObjectField> EnterObjectField{ get; set; }

        public NodeVisitor<GraphQLObjectValue> EnterObjectValue{ get; set; }

        public NodeVisitor<GraphQLOperationDefinition> EnterOperationDefinition{ get; set; }

        public NodeVisitor<GraphQLSelectionSet> EnterSelectionSet{ get; set; }

        public NodeVisitor<GraphQLScalarValue> EnterStringValue{ get; set; }

        public NodeVisitor<GraphQLVariable> EnterVariable{ get; set; }

        public NodeVisitor<GraphQLVariableDefinition> EnterVariableDefinition{ get; set; }

        public NodeVisitor<GraphQLArgument> LeaveArgument{ get; set; }

        public NodeVisitor<GraphQLDirective> LeaveDirective{ get; set; }

        public NodeVisitor<GraphQLDocument> LeaveDocument{ get; set; }

        public NodeVisitor<GraphQLScalarValue> LeaveEnumValue{ get; set; }

        public NodeVisitor<GraphQLFieldSelection> LeaveFieldSelection{ get; set; }

        public NodeVisitor<GraphQLFragmentDefinition> LeaveFragmentDefinition{ get; set; }

        public NodeVisitor<GraphQLInlineFragment> LeaveInlineFragment{ get; set; }

        public NodeVisitor<GraphQLListValue> LeaveListValue{ get; set; }

        public NodeVisitor<GraphQLObjectField> LeaveObjectField{ get; set; }

        public NodeVisitor<GraphQLObjectValue> LeaveObjectValue{ get; set; }

        public NodeVisitor<GraphQLOperationDefinition> LeaveOperationDefinition{ get; set; }

        public NodeVisitor<GraphQLSelectionSet> LeaveSelectionSet{ get; set; }

        public NodeVisitor<GraphQLVariable> LeaveVariable{ get; set; }

        public NodeVisitor<GraphQLVariableDefinition> LeaveVariableDefinition{ get; set; }
    }
}