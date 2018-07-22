using System;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.error
{
    public class FieldErrorException : Exception
    {
        public ComplexType ObjectType { get; }

        public string FieldName { get; }

        public IGraphQLType FieldType { get; }

        public GraphQLFieldSelection FieldSelection { get; }

        public object CompletedValue { get; }

        public FieldErrorException(string message, ComplexType objectType, string fieldName, IGraphQLType fieldType, GraphQLFieldSelection fieldSelection, object completedValue, Exception exception)
            :base(message, exception)
        {
            ObjectType = objectType;
            FieldName = fieldName;
            FieldType = fieldType;
            FieldSelection = fieldSelection;
            CompletedValue = completedValue;
        }
    }
}