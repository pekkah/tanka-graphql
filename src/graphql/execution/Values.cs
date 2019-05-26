using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using tanka.graphql.error;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public static class Values
    {
        public static object CoerceValue(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            object value,
            IType valueType)
        {
            if (valueType is NonNull nonNull) return CoerceNonNullValue(getInputObjectFields, value, nonNull);

            if (valueType is List list) return CoerceListValues(getInputObjectFields, list.WrappedType, value);

            if (valueType is ScalarType scalar) return CoerceScalarValue(value, scalar);

            if (valueType is EnumType enumType) return CoerceEnumValue(value, enumType);

            if (valueType is InputObjectType input) return CoerceInputValue(getInputObjectFields, value, input);

            throw new ValueCoercionException(
                $"Unexpected valueType {valueType}. Cannot coerce value.",
                value,
                valueType);
        }

        private static IDictionary<string, object> CoerceInputValue(Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields, object value, InputObjectType input)
        {
            if (value == null)
                return null;

            var result = new Dictionary<string, object>();

            if (value is GraphQLObjectValue objectValue)
            {
                return CoerceInputValueAst(getInputObjectFields, input, objectValue, result);
            }

            if (value is IDictionary<string, object> dictionaryValues)
            {
                var fields = getInputObjectFields(input.Name);
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

                    var coercedFieldValue = CoerceValue(getInputObjectFields, astValue, fieldType);

                    result[fieldName] = coercedFieldValue;
                }
            }

            return result;
        }

        private static IDictionary<string, object> CoerceInputValueAst(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields, 
            InputObjectType input, 
            GraphQLObjectValue graphQLObjectValue,
            Dictionary<string, object> result)
        {
            var fields = getInputObjectFields(input.Name);
            foreach (var inputField in fields)
            {
                var fieldName = inputField.Key;
                var field = inputField.Value;
                var fieldType = field.Type;

                var astValue = graphQLObjectValue.Fields.SingleOrDefault(v => v.Name.Value == fieldName);
                var coercedFieldValue = CoerceValue(getInputObjectFields, astValue?.Value, fieldType);

                result[fieldName] = coercedFieldValue;
            }

            return result;
        }

        private static object CoerceNonNullValue(Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields, object value, NonNull nonNull)
        {
            var coercedValue = CoerceValue(getInputObjectFields, value, nonNull.WrappedType);
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

        private static object CoerceListValues(Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields, IType listWrappedType, object value)
        {
            var coercedListValues = new List<object>();
            if (value is GraphQLListValue listValue)
            {
                foreach (var listValueValue in listValue.Values)
                {
                    var coercedValue = CoerceValue(getInputObjectFields, listValueValue, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues.ToArray();
            }

            if (value is IEnumerable values)
            {
                foreach (var v in values)
                {
                    var coercedValue = CoerceValue(getInputObjectFields, v, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues.ToArray();
            }

            coercedListValues.Add(CoerceValue(getInputObjectFields, value, listWrappedType));
            return coercedListValues.ToArray();
        }
    }
}