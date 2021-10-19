using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public static class TypeSystemDirectiveLocations
    {
        public const string SCHEMA = nameof(SCHEMA);
        public const string SCALAR = nameof(SCALAR);
        public const string OBJECT = nameof(OBJECT);
        public const string FIELD_DEFINITION = nameof(FIELD_DEFINITION);
        public const string ARGUMENT_DEFINITION = nameof(ARGUMENT_DEFINITION);
        public const string INTERFACE = nameof(INTERFACE);
        public const string UNION = nameof(UNION);
        public const string ENUM = nameof(ENUM);
        public const string ENUM_VALUE = nameof(ENUM_VALUE);
        public const string INPUT_OBJECT = nameof(INPUT_OBJECT);
        public const string INPUT_FIELD_DEFINITION = nameof(INPUT_FIELD_DEFINITION);

        public static IReadOnlyList<string> All = new List<string>
        {
            SCHEMA,
            SCALAR,
            OBJECT,
            FIELD_DEFINITION,
            ARGUMENT_DEFINITION,
            INTERFACE,
            UNION,
            ENUM,
            ENUM_VALUE,
            INPUT_OBJECT,
            INPUT_FIELD_DEFINITION
        };
    }
}