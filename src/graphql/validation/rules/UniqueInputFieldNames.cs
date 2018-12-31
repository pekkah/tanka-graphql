using System;
using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    /// Unique input field names
    ///
    /// A GraphQL input object value is only valid if all supplied fields are
    /// uniquely named.
    /// </summary>
    public class UniqueInputFieldNames : IValidationRule
    {
        public Func<string, string> DuplicateInputField =
            fieldName => $"There can be only one input field named {fieldName}.";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var knownNameStack = new Stack<Dictionary<string, GraphQLValue>>();
            var knownNames = new Dictionary<string, GraphQLValue>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLObjectValue>(
                    enter: objVal =>
                    {
                        knownNameStack.Push(knownNames);
                        knownNames = new Dictionary<string, GraphQLValue>();
                    },
                    leave: objVal =>
                    {
                        knownNames = knownNameStack.Pop();
                    });

                _.Match<GraphQLObjectField>(
                    leave: objField =>
                    {
                        if (knownNames.ContainsKey(objField.Name.Value))
                        {
                            context.ReportError(new ValidationError(
                                DuplicateInputField(objField.Name.Value),
                                knownNames[objField.Name.Value],
                                objField.Value));
                        }
                        else
                        {
                            knownNames[objField.Name.Value] = objField.Value;
                        }
                    });
            });
        }
    }
}
