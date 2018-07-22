using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Unique operation names
    ///     A GraphQL document is only valid if all defined operations have unique names.
    /// </summary>
    public class UniqueOperationNames : IValidationRule
    {
        public Func<string, string> DuplicateOperationNameMessage => opName =>
            $"There can only be one operation named {opName}.";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var frequency = new Dictionary<string, string>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLOperationDefinition>(
                    op =>
                    {
                        if (context.Document.Definitions.OfType<GraphQLOperationDefinition>().Count() < 2) 
                            return;
                        
                        if (string.IsNullOrWhiteSpace(op.Name?.Value)) return;

                        if (frequency.ContainsKey(op.Name.Value))
                        {
                            var error = new ValidationError(
                                DuplicateOperationNameMessage(op.Name.Value),
                                op);
                            context.ReportError(error);
                        }
                        else
                        {
                            frequency[op.Name.Value] = op.Name.Value;
                        }
                    });
            });
        }
    }
}