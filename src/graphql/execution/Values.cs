using System.Collections;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public static class Values
    {
        public static object CoerceValue(
            object value,
            IGraphQLType valueType)
        {
            if (valueType is NonNull nonNull) return CoerceNonNullValue(value, nonNull);

            if (valueType is List list) return CoerceListValues(list.WrappedType, value);

            if (valueType is ScalarType scalar) return CoerceScalarValue(value, scalar);

            if (valueType is EnumType enumType) return CoerceEnumValue(value, enumType);

            if (valueType is InputObjectType input) return CoerceInputValue(value, input);

            throw new ValueCoercionException(
                $"Unexpected valueType {valueType.Name}. Cannot coerce value.",
                value,
                valueType);
        }

        private static IDictionary<string, object> CoerceInputValue(object value, InputObjectType input)
        {
            var result = new Dictionary<string, object>();

            if (value is GraphQLObjectValue objectValue)
            {
                return CoerceInputValueAst(input, objectValue, result);
            }

            if (value is IDictionary<string, object> dictionaryValues)
            {
                foreach (var inputField in input.Fields)
                {
                    var fieldName = inputField.Key;
                    var field = inputField.Value;
                    var fieldType = field.Type;

                    object astValue = null;

                    if (dictionaryValues.ContainsKey(fieldName))
                    {
                        astValue = dictionaryValues[fieldName];
                    }

                    var coercedFieldValue = CoerceValue(astValue, fieldType);

                    result[fieldName] = coercedFieldValue;
                }
            }

            return result;
        }

        private static IDictionary<string, object> CoerceInputValueAst(InputObjectType input, GraphQLObjectValue graphQLObjectValue,
            Dictionary<string, object> result)
        {
            foreach (var inputField in input.Fields)
            {
                var fieldName = inputField.Key;
                var field = inputField.Value;
                var fieldType = field.Type;

                var astValue = graphQLObjectValue.Fields.SingleOrDefault(v => v.Name.Value == fieldName);
                var coercedFieldValue = CoerceValue(astValue.Value, fieldType);

                result[fieldName] = coercedFieldValue;
            }

            return result;
        }

        private static object CoerceNonNullValue(object value, NonNull nonNull)
        {
            var coercedValue = CoerceValue(value, nonNull.WrappedType);
            if (coercedValue == null)
                throw new ValueCoercionException("Coerced value is null", 
                    value,
                    nonNull);

            return coercedValue;
        }

        private static object CoerceEnumValue(object value, EnumType enumType1)
        {
            if (value is GraphQLScalarValue astValue) 
                return enumType1.ParseLiteral(astValue);

            return enumType1.ParseValue(value);
        }

        private static object CoerceScalarValue(object value, ScalarType scalarType)
        {
            if (value is GraphQLScalarValue astValue) 
                return scalarType.ParseLiteral(astValue);

            return scalarType.ParseValue(value);
        }

        private static object CoerceListValues(IGraphQLType listWrappedType, object value)
        {
            var coercedListValues = new List<object>();
            if (value is GraphQLListValue listValue)
            {
                foreach (var listValueValue in listValue.Values)
                {
                    var coercedValue = CoerceValue(listValueValue, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues;
            }

            if (value is IEnumerable values)
            {
                foreach (var v in values)
                {
                    var coercedValue = CoerceValue(v, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues;
            }

            coercedListValues.Add(CoerceValue(value, listWrappedType));
            return coercedListValues;
        }
    }
}