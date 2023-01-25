using System.Collections;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL;

public static class Values
{
    public static object? CoerceValue(
        ISchema schema,
        object? value,
        TypeBase valueType)
    {
        switch (valueType)
        {
            case NonNullType nonNullType:
                return CoerceNonNullTypeValue(
                    schema,
                    value,
                    nonNullType);
            case ListType list:
                return CoerceListValues(
                    schema,
                    list.OfType,
                    value);
        }


        if (valueType is not NamedType namedValueType)
            throw new ValueCoercionException(
                $"Unexpected valueType {valueType}. Cannot coerce value.",
                value,
                valueType);

        var valueTypeDefinition = schema.GetRequiredNamedType<TypeDefinition>(namedValueType.Name);

        return valueTypeDefinition switch
        {
            ScalarDefinition scalar => CoerceScalarValue(schema, value, scalar),
            EnumDefinition enumDefinition => CoerceEnumValue(value, enumDefinition),
            InputObjectDefinition input => CoerceInputValue(
                schema,
                value,
                input),
            _ => throw new ArgumentOutOfRangeException($"Type of the '{valueType} is not supported by value coercion")
        };
    }

    private static IReadOnlyDictionary<string, object?>? CoerceInputValue(
        ISchema schema,
        object? value,
        InputObjectDefinition input)
    {
        if (value == null)
            return null;

        var result = new Dictionary<string, object?>();

        if (value is ObjectValue objectValue)
            return CoerceInputValueAst(schema, input, objectValue, result);

        if (value is IDictionary<string, object?> dictionaryValues)
        {
            var fields = schema.GetInputFields(input.Name);
            foreach (var inputField in fields)
            {
                var fieldName = inputField.Key;
                var field = inputField.Value;
                var fieldType = field.Type;

                object? astValue = null;

                if (dictionaryValues.ContainsKey(fieldName))
                    astValue = dictionaryValues[fieldName];

                var coercedFieldValue = CoerceValue(schema, astValue, fieldType);

                result[fieldName] = coercedFieldValue;
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<string, object?> CoerceInputValueAst(
        ISchema schema,
        InputObjectDefinition input,
        ObjectValue objectValue,
        Dictionary<string, object?> result)
    {
        var fields = schema.GetInputFields(input.Name);
        var valueFields = objectValue.Fields.ToDictionary(f => f.Name.Value, f => f);

        foreach (var inputField in fields)
        {
            var fieldName = inputField.Key;
            var field = inputField.Value;
            var fieldType = field.Type;

            var astValue = valueFields.GetValueOrDefault(fieldName);
            var coercedFieldValue = CoerceValue(schema, astValue?.Value, fieldType);

            result[fieldName] = coercedFieldValue;
        }

        return result;
    }

    private static object CoerceNonNullTypeValue(
        ISchema schema,
        object? value,
        NonNullType nonNullType)
    {
        var coercedValue = CoerceValue(schema, value, nonNullType.OfType);
        if (coercedValue == null)
            throw new ValueCoercionException("Coerced value is null",
                value,
                nonNullType);

        return coercedValue;
    }

    private static object? CoerceEnumValue(object? value, EnumDefinition enumType)
    {
        if (value is ValueBase astValue)
            return new EnumConverter(enumType).ParseLiteral(astValue);

        return new EnumConverter(enumType).ParseValue(value);
    }

    private static object? CoerceScalarValue(
        ISchema schema,
        object? value,
        ScalarDefinition scalarType)
    {
        var serializer = schema.GetRequiredValueConverter(scalarType.Name);

        if (value is ValueBase astValue)
            return serializer.ParseLiteral(astValue);

        return serializer.ParseValue(value);
    }

    private static object? CoerceListValues(
        ISchema schema,
        TypeBase listWrappedType,
        object? value)
    {
        if (value == null)
            return null;

        var coercedListValues = new List<object?>();
        if (value is ListValue listValue)
        {
            foreach (var listValueValue in listValue)
            {
                var coercedValue = CoerceValue(schema, listValueValue,
                    listWrappedType);
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
        return coercedListValues.ToArray();
    }
}