using System;
using System.Linq;
using tanka.graphql.schema;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class IntrospectionSchema
    {
        public const string EnumValueName = "__EnumValue";
        public const string FieldName = "__Field";
        public const string InputValueName = "__InputValue";
        public const string SchemaName = "__Schema";
        public const string TypeKindName = "__TypeKind";
        public const string TypeName = "__Type";

        public static SchemaBuilder Create()
        {
            var builder = new SchemaBuilder();

            // define type here so that it can be referenced early
            builder.Object(TypeName, out var type);
            var typeList = new List(new NonNull(type));

            builder.Enum(TypeKindName, out var typeKind,
                null,
                null,
                (__TypeKind.SCALAR.ToString(), default, null, null),
                (__TypeKind.OBJECT.ToString(), default, null, null),
                (__TypeKind.ENUM.ToString(), default, null, null),
                (__TypeKind.INPUT_OBJECT.ToString(), default, null, null),
                (__TypeKind.INTERFACE.ToString(), default, null, null),
                (__TypeKind.LIST.ToString(), default, null, null),
                (__TypeKind.NON_NULL.ToString(), default, null, null),
                (__TypeKind.UNION.ToString(), default, null, null));

            builder.Object(InputValueName, out var inputValue)
                .Connections(connect => connect
                    .Field(inputValue, "name", ScalarType.NonNullString)
                    .Field(inputValue, "description", ScalarType.String)
                    .Field(inputValue, "defaultValue", ScalarType.String)
                    .Field(inputValue, "type", new NonNull(type)));

            var inputValueList = new List(new NonNull(inputValue));
            var argsList = new List(new NonNull(inputValue));

            builder.Object(FieldName, out var field)
                .Connections(connect => connect
                    .Field(field, "name", ScalarType.NonNullString)
                    .Field(field, "description", ScalarType.String)
                    .Field(field, "args", argsList)
                    .Field(field, "type", new NonNull(type))
                    .Field(field, "isDeprecated", ScalarType.NonNullBoolean)
                    .Field(field, "deprecationReason", ScalarType.String));

            var fieldList = new List(new NonNull(field));

            builder.Object(EnumValueName, out var enumValue)
                .Connections(connect => connect
                    .Field(enumValue, "name", ScalarType.NonNullString)
                    .Field(enumValue, "description", ScalarType.String)
                    .Field(enumValue, "isDeprecated", ScalarType.NonNullBoolean)
                    .Field(enumValue, "deprecationReason", ScalarType.String));

            var enumValueList = new List(new NonNull(enumValue));

            builder
                .Connections(connect => connect
                    .Field(type, "kind", new NonNull(typeKind))
                    .Field(type, "name", ScalarType.String)
                    .Field(type, "description", ScalarType.String)
                    .Field(type, "fields", fieldList,
                        args: ("includeDeprecated", ScalarType.Boolean, false, default))
                    .Field(type, "interfaces", typeList)
                    .Field(type, "possibleTypes", typeList)
                    .Field(type, "enumValues", enumValueList,
                        args: ("includeDeprecated", ScalarType.Boolean, false, default))
                    .Field(type, "inputFields", inputValueList)
                    .Field(type, "ofType", type));

            builder.Enum("__DirectiveLocation", out var directiveLocation,
                directives: null,
                values: Enum.GetNames(typeof(__DirectiveLocation))
                    .Select(loc => (loc, string.Empty, Enumerable.Empty<DirectiveInstance>(), string.Empty))
                    .ToArray());

            builder.Object("__Directive", out var directive)
                .Connections(connect => connect
                    .Field(directive, "name", ScalarType.String)
                    .Field(directive, "description", ScalarType.String)
                    .Field(directive, "locations", new List(directiveLocation))
                    .Field(directive, "args", argsList));

            builder.Object(SchemaName, out var schema)
                .Connections(connect => connect
                    .Field(schema, "types", typeList)
                    .Field(schema, "queryType", type)
                    .Field(schema, "mutationType", type)
                    .Field(schema, "subscriptionType", type)
                    .Field(schema, "directives", new List(directive)));

            builder.Query(out var query)
                .Connections(connect => connect
                    .Field(query, "__schema", schema)
                    .Field(query, "__type", type,
                        args: ("name", ScalarType.NonNullString, default, default)));

            return builder;
        }
    }
}