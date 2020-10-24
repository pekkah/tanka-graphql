using System.Diagnostics.CodeAnalysis;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.ApolloFederation
{
    public static class FederationTypes
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static readonly ScalarType _Any = new ScalarType("_Any");

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static readonly ScalarType _FieldSet = new ScalarType("_FieldSet");

        public static readonly DirectiveType External = new DirectiveType(
            "external",
            new[] {DirectiveLocation.FIELD_DEFINITION});

        public static readonly DirectiveType Requires = new DirectiveType(
            "requires",
            new[] {DirectiveLocation.FIELD_DEFINITION},
            new Args
            {
                {"fields", _FieldSet, null, "Fields"}
            });

        public static readonly DirectiveType Provides = new DirectiveType(
            "provides",
            new[] {DirectiveLocation.FIELD_DEFINITION},
            new Args
            {
                {"fields", _FieldSet, null, "Fields"}
            });

        public static readonly DirectiveType Key = new DirectiveType(
            "key",
            new[] {DirectiveLocation.FIELD_DEFINITION},
            new Args
            {
                {"fields", _FieldSet, null, "fields"}
            });

        public static readonly DirectiveType Extends = new DirectiveType(
            "extends",
            new[] {DirectiveLocation.OBJECT, DirectiveLocation.INTERFACE});
    }
}