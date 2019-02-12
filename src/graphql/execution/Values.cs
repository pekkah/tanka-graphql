using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.error;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public static class Values
    {
        public static object CoerceValue(
            ISchema schema,
            object value,
            IType valueType)
        {
            if (valueType is NonNull nonNull) return CoerceNonNullValue(schema, value, nonNull);

            if (valueType is List list) return CoerceListValues(schema, list.WrappedType, value);

            if (valueType is ScalarType scalar) return CoerceScalarValue(value, scalar);

            if (valueType is EnumType enumType) return CoerceEnumValue(value, enumType);

            if (valueType is InputObjectType input) return CoerceInputValue(schema, value, input);

            throw new ValueCoercionException(
                $"Unexpected valueType {valueType}. Cannot coerce value.",
                value,
                valueType);
        }

        private static IDictionary<string, object> CoerceInputValue(ISchema schema, object value, InputObjectType input)
        {
            var result = new Dictionary<string, object>();

            if (value is GraphQLObjectValue objectValue)
            {
                return CoerceInputValueAst(schema, input, objectValue, result);
            }

            if (value is IDictionary<string, object> dictionaryValues)
            {
                var fields = schema.GetInputFields(input.Name);
                foreach (var inputField in fields)
                {
                    var fieldName = inputField.Key;
                    var field = inputField.Value;
                    var fieldType = field.Type;

                    object astValue = null;

                    if (dictionaryValues.ContainsKey(fieldName))
                    {
                        astValue = dictionaryValues[fieldName];
                    }

                    var coercedFieldValue = CoerceValue(schema, astValue, fieldType);

                    result[fieldName] = coercedFieldValue;
                }
            }

            return result;
        }

        private static IDictionary<string, object> CoerceInputValueAst(
            ISchema schema, 
            InputObjectType input, 
            GraphQLObjectValue graphQLObjectValue,
            Dictionary<string, object> result)
        {
            var fields = schema.GetInputFields(input.Name);
            foreach (var inputField in fields)
            {
                var fieldName = inputField.Key;
                var field = inputField.Value;
                var fieldType = field.Type;

                var astValue = graphQLObjectValue.Fields.SingleOrDefault(v => v.Name.Value == fieldName);
                var coercedFieldValue = CoerceValue(schema, astValue.Value, fieldType);

                result[fieldName] = coercedFieldValue;
            }

            return result;
        }

        private static object CoerceNonNullValue(ISchema schema, object value, NonNull nonNull)
        {
            var coercedValue = CoerceValue(schema, value, nonNull.WrappedType);
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

        private static object CoerceListValues(ISchema schema, IType listWrappedType, object value)
        {
            var coercedListValues = new List<object>();
            if (value is GraphQLListValue listValue)
            {
                foreach (var listValueValue in listValue.Values)
                {
                    var coercedValue = CoerceValue(schema, listValueValue, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues;
            }

            if (value is IEnumerable values)
            {
                foreach (var v in values)
                {
                    var coercedValue = CoerceValue(schema, v, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues;
            }

            coercedListValues.Add(CoerceValue(schema, value, listWrappedType));
            return coercedListValues;
        }
    }
}