using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.Execution
{
    public static class Values
    {
        public static object CoerceValue(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            Func<string, IValueConverter> getValueConverter,
            object value,
            IType valueType)
        {
            if (valueType is NonNull nonNull)
                return CoerceNonNullValue(
                    getInputObjectFields,
                    getValueConverter,
                    value,
                    nonNull);

            if (valueType is List list)
                return CoerceListValues(
                    getInputObjectFields,
                    getValueConverter,
                    list.OfType,
                    value);

            if (valueType is ScalarType scalar) return CoerceScalarValue(getValueConverter, value, scalar);

            if (valueType is EnumType enumType) return CoerceEnumValue(value, enumType);

            if (valueType is InputObjectType input)
                return CoerceInputValue(
                    getInputObjectFields,
                    getValueConverter,
                    value,
                    input);

            throw new ValueCoercionException(
                $"Unexpected valueType {valueType}. Cannot coerce value.",
                value,
                valueType);
        }

        private static IDictionary<string, object> CoerceInputValue(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            Func<string, IValueConverter> getValueConverter, 
            object value,
            InputObjectType input)
        {
            if (value == null)
                return null;

            var result = new Dictionary<string, object>();

            if (value is GraphQLObjectValue objectValue)
                return CoerceInputValueAst(getInputObjectFields, getValueConverter, input, objectValue, result);

            if (value is IDictionary<string, object> dictionaryValues)
            {
                var fields = getInputObjectFields(input.Name);
                foreach (var inputField in fields)
                {
                    var fieldName = inputField.Key;
                    var field = inputField.Value;
                    var fieldType = field.Type;

                    object astValue = null;

                    if (dictionaryValues.ContainsKey(fieldName)) astValue = dictionaryValues[fieldName];

                    var coercedFieldValue = CoerceValue(getInputObjectFields, getValueConverter, astValue, fieldType);

                    result[fieldName] = coercedFieldValue;
                }
            }

            return result;
        }

        private static IDictionary<string, object> CoerceInputValueAst(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            Func<string, IValueConverter> getValueConverter,
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
                var coercedFieldValue =
                    CoerceValue(getInputObjectFields, getValueConverter, astValue?.Value, fieldType);

                result[fieldName] = coercedFieldValue;
            }

            return result;
        }

        private static object CoerceNonNullValue(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            Func<string, IValueConverter> getValueConverter,
            object value,
            NonNull nonNull)
        {
            var coercedValue = CoerceValue(getInputObjectFields, getValueConverter, value, nonNull.OfType);
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

        private static object CoerceScalarValue(
            Func<string, IValueConverter> getValueConverter,
            object value,
            ScalarType scalarType)
        {
            var serializer = getValueConverter(scalarType.Name);

            if (value is GraphQLScalarValue astValue)
                return serializer.ParseLiteral(astValue);

            return serializer.ParseValue(value);
        }

        private static object CoerceListValues(
            Func<string, IEnumerable<KeyValuePair<string, InputObjectField>>> getInputObjectFields,
            Func<string, IValueConverter> getValueConverter,
            IType listWrappedType,
            object value)
        {
            if (value == null)
                return null;

            var coercedListValues = new List<object>();
            if (value is GraphQLListValue listValue)
            {
                foreach (var listValueValue in listValue.Values)
                {
                    var coercedValue = CoerceValue(getInputObjectFields, getValueConverter, listValueValue,
                        listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues.ToArray();
            }

            if (value is IEnumerable values)
            {
                foreach (var v in values)
                {
                    var coercedValue = CoerceValue(getInputObjectFields, getValueConverter, v, listWrappedType);
                    coercedListValues.Add(coercedValue);
                }

                return coercedListValues.ToArray();
            }

            coercedListValues.Add(CoerceValue(getInputObjectFields, getValueConverter, value, listWrappedType));
            return coercedListValues.ToArray();
        }
    }
}