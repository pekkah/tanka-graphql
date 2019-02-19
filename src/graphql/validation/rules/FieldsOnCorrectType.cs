using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Fields on correct type
    ///     A GraphQL document is only valid if all Fields selected are defined by the
    ///     parent type, or are an allowed meta Fields such as __typename
    /// </summary>
    public class FieldsOnCorrectType : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFieldSelection>(node =>
                {
                    var type = context.TypeInfo.GetParentType().Unwrap();

                    if (type != null)
                    {
                        var field = context.TypeInfo.GetFieldDef();
                        if (field == null && node.Name.Value != "__typename")
                        {
                            // This Fields doesn't exist, lets look for suggestions.
                            var fieldName = node.Name.Value;

                            // First determine if there are any suggested types to condition on.
                            var suggestedTypeNames = GetSuggestedTypeNames(
                                context.Schema,
                                type,
                                fieldName).ToList();

                            // If there are no suggested types, then perhaps this was a typo?
                            var suggestedGraphQLFieldSelectionNames = suggestedTypeNames.Any()
                                ? new string[] { }
                                : GetSuggestedGraphQLFieldSelectionNames(type, fieldName);

                            // Report an error, including helpful suggestions.
                            context.ReportError(new ValidationError(
                                UndefinedGraphQLFieldSelectionMessage(fieldName, type,
                                    suggestedTypeNames, suggestedGraphQLFieldSelectionNames),
                                node
                            ));
                        }
                    }
                });
            });
        }

        public string UndefinedGraphQLFieldSelectionMessage(
            string field,
            IType type,
            IEnumerable<string> suggestedTypeNames,
            IEnumerable<string> suggestedGraphQLFieldSelectionNames)
        {
            var message = $"Cannot query Fields \"{field}\" on type \"{type}\".";

            if (suggestedTypeNames != null && suggestedTypeNames.Any())
            {
                var suggestions = string.Join(",", suggestedTypeNames);
                message += $" Did you mean to use an inline fragment on {suggestions}?";
            }
            else if (suggestedGraphQLFieldSelectionNames != null && suggestedGraphQLFieldSelectionNames.Any())
            {
                message += $" Did you mean {string.Join(",", suggestedGraphQLFieldSelectionNames)}?";
            }

            return message;
        }

        /// <summary>
        ///     Go through all of the implementations of type, as well as the interfaces
        ///     that they implement. If any of those types include the provided GraphQLFieldSelection,
        ///     suggest them, sorted by how often the type is referenced,  starting
        ///     with Interfaces.
        /// </summary>
        private IEnumerable<string> GetSuggestedTypeNames(
            ISchema schema,
            IType type,
            string graphQLFieldSelectionName)
        {
            /*
            if (type is InterfaceType)
            {
                var suggestedObjectTypes = new List<string>();
                var interfaceUsageCount = new LightweightCache<string, int>(key => 0);

                var absType = type as IAbstractGraphType;
                absType.PossibleTypes.Apply(possibleType =>
                {
                    if (!possibleType.HasGraphQLFieldSelection(graphQLFieldSelectionName))
                    {
                        return;
                    }

                    // This object defines this GraphQLFieldSelection.
                    suggestedObjectTypes.Add(possibleType.Name);

                    possibleType.ResolvedInterfaces.Apply(possibleInterface =>
                    {
                        if (possibleInterface.HasGraphQLFieldSelection(graphQLFieldSelectionName))
                        {
                            // This interface type defines this GraphQLFieldSelection.
                            interfaceUsageCount[possibleInterface.Name] = interfaceUsageCount[possibleInterface.Name] + 1;
                        }
                    });
                });

                var suggestedInterfaceTypes = interfaceUsageCount.Keys.OrderBy(x => interfaceUsageCount[x]);
                return suggestedInterfaceTypes.Concat(suggestedObjectTypes);
            }*/

            return Enumerable.Empty<string>();
        }

        /// <summary>
        ///     For the GraphQLFieldSelection name provided, determine if there are any similar GraphQLFieldSelection names
        ///     that may be the result of a typo.
        /// </summary>
        private IEnumerable<string> GetSuggestedGraphQLFieldSelectionNames(
            IType type,
            string graphQLFieldSelectionName)
        {
            /*
            if (type is InterfaceType)
            {
                var complexType = type as IComplexGraphType;
                return StringUtils.SuggestionList(graphQLFieldSelectionName, complexType.GraphQLFieldSelections.Select(x => x.Name));
            }*/

            return Enumerable.Empty<string>();
        }
    }
}