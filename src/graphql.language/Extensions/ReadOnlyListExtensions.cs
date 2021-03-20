using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class ReadOnlyListExtensions
    {
        public static IReadOnlyList<T>? Concat<T>(this IReadOnlyList<T>? left, IReadOnlyList<T>? right)
        {
            if (left is null && right is null)
                return null;

            if (left is null != right is null)
                return left ?? right;

            var result = left?.ToList() ?? new List<T>();

            if (right is not null)
            {
                result.AddRange(right);
            }

            return result;
        }

        public static Directives? Concat(this Directives? left, Directives? right)
        {
            var directives = Concat(left as IReadOnlyList<Directive>, right);

            if (directives is null)
                return null;

            return new Directives(directives, left?.Location ?? right?.Location);
        }

        public static IReadOnlyList<T>? Join<T, TKey>(
            this IReadOnlyList<T>? left, 
            IReadOnlyList<T>? right, 
            Func<T, TKey> keySelector,
            Func<T, T, T> resultSelector)
        {
            if (left is null && right is null)
                return null;

            if (left is null != right is null)
                return left ?? right;

            var result = left?.ToList() ?? new List<T>();

            if (right is not null)
            {
                return result.Join(
                    right,
                    keySelector,
                    keySelector,
                    resultSelector).ToList();
            }

            return result;
        }

        public static IReadOnlyList<NamedType>? Join(
            this IReadOnlyList<NamedType>? left,
            IReadOnlyList<NamedType>? right)
        {
            return Join(
                left,
                right,
                namedType => namedType.Name.Value,
                (leftItem, _) => leftItem);
        }

        public static FieldsDefinition? Join(
            this FieldsDefinition? left,
            FieldsDefinition? right)
        {
            if (left is null && right is null)
                return null;

            if (left is null != right is null)
                return left ?? right;

            var fields = Join(
                left,
                right,
                field => field.Name.Value,
                (leftField, _) => leftField);

            if (fields is null)
                return null;

            return new FieldsDefinition(fields, left?.Location ?? right?.Location);
        }

        public static InputFieldsDefinition? Join(
            this InputFieldsDefinition? left,
            InputFieldsDefinition? right)
        {
            if (left is null && right is null)
                return null;

            if (left is null != right is null)
                return left ?? right;

            var fields = Join(
                left,
                right,
                field => field.Name.Value,
                (leftField, _) => leftField);

            if (fields is null)
                return null;

            return new InputFieldsDefinition(fields, left?.Location ?? right?.Location);
        }

        public static EnumValuesDefinition? Join(
            this EnumValuesDefinition? left,
            EnumValuesDefinition? right)
        {
            if (left is null && right is null)
                return null;

            if (left is null != right is null)
                return left ?? right;

            var values = Join(
                left,
                right,
                value => value.Value.Name.Value,
                (leftValue, _) => leftValue);

            if (values is null)
                return null;

            return new EnumValuesDefinition(values, left?.Location ?? right?.Location);
        }
    }
}