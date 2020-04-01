

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation
{
    public class RuleVisitor
    {
        public NodeVisitor<Argument> EnterArgument { get; set; }

        public NodeVisitor<BooleanValue> EnterBooleanValue { get; set; }

        public NodeVisitor<Directive> EnterDirective { get; set; }

        public NodeVisitor<Directive> EnterDirectives { get; set; }

        public NodeVisitor<ExecutableDocument> EnterDocument { get; set; }

        public NodeVisitor<EnumValue> EnterEnumValue { get; set; }

        public NodeVisitor<FieldSelection> EnterFieldSelection { get; set; }

        public NodeVisitor<FloatValue> EnterFloatValue { get; set; }

        public NodeVisitor<FragmentDefinition> EnterFragmentDefinition { get; set; }

        public NodeVisitor<FragmentSpread> EnterFragmentSpread { get; set; }

        public NodeVisitor<InlineFragment> EnterInlineFragment { get; set; }

        public NodeVisitor<IntValue> EnterIntValue { get; set; }

        public NodeVisitor<ListValue> EnterListValue { get; set; }
        
        public NodeVisitor<FieldSelection> EnterObjectField { get; set; }
        
        public NodeVisitor<FieldSelection> LeaveObjectField { get; set; }

        public NodeVisitor<NamedType> EnterNamedType{ get; set; }

        public NodeVisitor<INode> EnterNode{ get; set; }
        
        public NodeVisitor<ObjectValue> EnterObjectValue{ get; set; }

        public NodeVisitor<OperationDefinition> EnterOperationDefinition{ get; set; }

        public NodeVisitor<SelectionSet> EnterSelectionSet{ get; set; }

        public NodeVisitor<StringValue> EnterStringValue{ get; set; }

        public NodeVisitor<Variable> EnterVariable{ get; set; }

        public NodeVisitor<VariableDefinition> EnterVariableDefinition{ get; set; }

        public NodeVisitor<Argument> LeaveArgument{ get; set; }

        public NodeVisitor<Directive> LeaveDirective{ get; set; }

        public NodeVisitor<ExecutableDocument> LeaveDocument{ get; set; }

        public NodeVisitor<EnumValue> LeaveEnumValue{ get; set; }

        public NodeVisitor<FieldSelection> LeaveFieldSelection{ get; set; }

        public NodeVisitor<FragmentDefinition> LeaveFragmentDefinition{ get; set; }

        public NodeVisitor<InlineFragment> LeaveInlineFragment{ get; set; }

        public NodeVisitor<ListValue> LeaveListValue{ get; set; }
        
        public NodeVisitor<ObjectValue> LeaveObjectValue{ get; set; }

        public NodeVisitor<OperationDefinition> LeaveOperationDefinition{ get; set; }

        public NodeVisitor<SelectionSet> LeaveSelectionSet{ get; set; }

        public NodeVisitor<Variable> LeaveVariable{ get; set; }

        public NodeVisitor<VariableDefinition> LeaveVariableDefinition{ get; set; }
    }
}