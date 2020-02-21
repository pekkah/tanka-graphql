using System;
using Microsoft.CodeAnalysis.CSharp;

namespace Tanka.GraphQL.Generator.Core
{
    public static class NameExtensions
    {
        public static string ToControllerName(this string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return $"{name}Controller";
        }

        public static string ToInterfaceName(this string name)
        {
            var capitalized = name
                .Capitalize();

            return $"I{capitalized}";
        }

        public static string ToModelName(this string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            return name
                .Capitalize();
        }

        public static string ToModelInterfaceName(this string name)
        {
            return name.ToModelName().ToInterfaceName();
        }

        public static string ToFieldResolversName(this string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return $"{name}Fields";
        }

        public static string ToSchemaResolversName(this string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return $"{name}Resolvers";
        }

        public static string ToFieldArgumentName(this string name)
        {
            return name.Sanitize();
        }

        public static string ToFieldResolverName(this string name)
        {
            return name
                .Capitalize();
        }

        public static string Sanitize(this string name)
        {
            if (name.IsKeyword()) return $"_{name}";

            return name;
        }

        public static bool IsKeyword(this string name)
        {
            var isAnyKeyword = SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
                               || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None;

            return isAnyKeyword;
        }

        public static string ToServiceBuilderName(this string name)
        {
            return $"{name.Capitalize()}ServicesBuilder";
        }

        private static string Capitalize(this string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            return $"{name.Substring(0, 1).ToUpperInvariant()}{name.Substring(1)}";
        }
    }
}