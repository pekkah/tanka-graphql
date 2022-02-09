using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;

namespace Tanka.GraphQL.TypeSystem
{
    public static class Scalars
    {
         public static ScalarDefinition Boolean = new ScalarDefinition(
            name: "Boolean",
            description: "The `Boolean` scalar type represents `true` or `false`");

        public static ScalarDefinition Float = new ScalarDefinition(
            name: "Float",
            description:"The `Float` scalar type represents signed double-precision fractional values" +
            " as specified by '[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point)");

        public static ScalarDefinition ID = new ScalarDefinition(
           name: "ID",
            description:"The ID scalar type represents a unique identifier, often used to refetch an object" +
            " or as the key for a cache. The ID type is serialized in the same way as a String; " +
            "however, it is not intended to be human‐readable. While it is often numeric, it " +
            "should always serialize as a String.");

        public static ScalarDefinition Int = new ScalarDefinition(
            name:"Int",
            description:"The `Int` scalar type represents non-fractional signed whole numeric values");

        public static ScalarDefinition String = new ScalarDefinition(
            name:"String",
            description:"The `String` scalar type represents textual data, represented as UTF-8" +
            " character sequences. The String type is most often used by GraphQL to" +
            " represent free-form human-readable text.");

        public static NonNullType NonNullBoolean = new NonNullType(new NamedType("Boolean"));
        public static NonNullType NonNullFloat = new NonNullType(new NamedType("Float"));
        public static NonNullType NonNullID = new NonNullType(new NamedType("ID"));
        public static NonNullType NonNullInt = new NonNullType(new NamedType("Int"));
        public static NonNullType NonNullString = new NonNullType(new NamedType("String"));

        public static IEnumerable<(ScalarDefinition Type, IValueConverter Converter)> Standard =
            new List<(ScalarDefinition Type, IValueConverter Converter)>()
            {
                (String, new StringConverter()),
                (Int, new IntConverter()),
                (Float, new DoubleConverter()),
                (Boolean, new BooleanConverter()),
                (ID, new IdConverter())
            };
    }
}