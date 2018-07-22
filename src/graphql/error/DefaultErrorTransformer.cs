using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using fugu.graphql.error;
using fugu.graphql.type;

namespace fugu.graphql.error
{
    public class DefaultErrorTransformer : IErrorTransformer
    {
        public IEnumerable<Error> Transfrom(Exception exception)
        {
            var message = GetExeptionMessage(exception);
            yield return new Error(message);

            var inner = exception.InnerException;

            while (inner != null)
            {
                if (inner is FieldErrorException)
                {
                    yield return new Error(GetExeptionMessage(inner));
                }

                inner = inner.InnerException;
            }
        }

        private string GetFieldExceptionMessage(ExecuteFieldException fieldException)
        {
            var objectType = fieldException.ObjectType;
            var objectTypeType = (IGraphQLType) objectType;
            var field = fieldException.Field;
            var fieldType = field.Type;
            var fieldName = objectType.GetFieldName(field);

            var builder = new StringBuilder();
            builder.Append($"Field '{objectTypeType.Name}.{fieldName}: {fieldType}' has an error. ");

            builder.Append(GetExeptionMessage(fieldException.InnerException));
            return builder.ToString();
        }

        private string GetExeptionMessage(Exception exception)
        {
            switch (exception)
            {
                case null:
                    return string.Empty;
                case FieldErrorException nonNullFieldValueNullException:
                    return GetFieldErrorMessage(nonNullFieldValueNullException);
                case ExecuteFieldException fieldException:
                    return GetFieldExceptionMessage(fieldException);
                case NullValueForNonNullTypeException nullValueException:
                    return GetNullValueForNonNullExceptionMessage(nullValueException);
                case ValueCoercionException coercionException:
                    return GetValueCoercionExceptionMessage(coercionException);
                case VariableException variableException:
                    return GetVariableExceptionMessage(variableException);
            }

            return exception.ToString();
        }

        private string GetFieldErrorMessage(
            FieldErrorException fieldErrorException)
        {
            var objectType = fieldErrorException.ObjectType;
            var field = objectType.GetField(fieldErrorException.FieldName);
            var fieldName = objectType.GetFieldName(field);

            var inner = string.Empty;
            if (!(fieldErrorException.InnerException is FieldErrorException))
            {
                inner = GetExeptionMessage(fieldErrorException.InnerException);
            }
            return
                $"'{objectType.Name}.{fieldName}: {field.Type}' is non-null field and cannot be set to null. {inner}";
        }

        private string GetVariableExceptionMessage(VariableException variableException)
        {
            return $"Cannot process variable '{variableException.VariableName}' " +
                   $"of type '{variableException.VariableType}'. " +
                   $"{variableException.Message}.";
        }

        private string GetValueCoercionExceptionMessage(ValueCoercionException coercionException)
        {
            return $"Cannot convert value '{coercionException.Value}' to type '{coercionException.Type}'";
        }

        private string GetNullValueForNonNullExceptionMessage(NullValueForNonNullTypeException nullValueException)
        {
            return $"Type '{nullValueException.Type}' cannot be null";
        }
    }
}