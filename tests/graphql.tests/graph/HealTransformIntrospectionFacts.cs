using System;
using tanka.graphql.introspection;
using tanka.graphql.type;
using Xunit;
using static tanka.graphql.type.Argument;
using static tanka.graphql.type.ScalarType;

namespace tanka.graphql.tests.graph
{
    public class HealTransformIntrospectionFacts
    {
        [Fact]
        public void Heal_introspection_schema()
        {
            /* Given */
            var typeKind = new NonNull(new EnumType(
                IntrospectionSchema.TypeKindName,
                new EnumValues
                {
                    [__TypeKind.SCALAR.ToString()] = null,
                    [__TypeKind.OBJECT.ToString()] = null,
                    [__TypeKind.ENUM.ToString()] = null,
                    [__TypeKind.INPUT_OBJECT.ToString()] = null,
                    [__TypeKind.INTERFACE.ToString()] = null,
                    [__TypeKind.LIST.ToString()] = null,
                    [__TypeKind.NON_NULL.ToString()] = null,
                    [__TypeKind.UNION.ToString()] = null
                }));

            var typeReference = new NamedTypeReference(IntrospectionSchema.TypeName);
            var nonNullTypeReference = new NonNull(typeReference);
            var typeListReference = new List(new NonNull(typeReference));

            var inputValue = new ObjectType(
                IntrospectionSchema.InputValueName,
                new Fields
                {
                    /*["name"] = new Field(NonNullString),
                    ["description"] = new Field(ScalarType.String),*/
                    ["type"] = new Field(nonNullTypeReference),
                    /*["defaultValue"] = new Field(ScalarType.String),*/
                });

            var argsList = new NonNull(new List(new NonNull(inputValue)));

            var field = new ObjectType(
                IntrospectionSchema.FieldName,
                new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["description"] = new Field(ScalarType.String),
                    ["args"] = new Field(argsList),
                    ["type"] = new Field(nonNullTypeReference),
                    ["isDeprecated"] = new Field(NonNullBoolean),
                    ["deprecationReason"] = new Field(ScalarType.String)
                });

            var fieldList = new List(new NonNull(field));

            var enumValue = new ObjectType(
                IntrospectionSchema.EnumValueName,
                new Fields
                {
                    ["name"] = new Field(NonNullString),
                    ["description"] = new Field(ScalarType.String),
                    ["isDeprecated"] = new Field(NonNullBoolean),
                    ["deprecationReason"] = new Field(ScalarType.String)
                });

            var enumValueList = new List(new NonNull(enumValue));

            var inputValueList = new List(new NonNull(inputValue));

            var type = new ObjectType(
                IntrospectionSchema.TypeName,
                new Fields
                {
                    /*["kind"] = new Field(typeKind),
                    ["name"] = new Field(ScalarType.String),
                    ["description"] = new Field(ScalarType.String),
                    ["fields"] = new Field(fieldList, new Args
                    {
                        ["includeDeprecated"] = Arg(ScalarType.Boolean, false)
                    }),
                   ["interfaces"] = new Field(typeListReference),
                   ["possibleTypes"] = new Field(typeListReference),
                    ["enumValues"] = new Field(enumValueList, new Args
                    {
                        ["includeDeprecated"] = Arg(ScalarType.Boolean, false)
                    }),*/
                    ["inputFields"] = new Field(inputValueList),
                    /*["ofType"] = new Field(typeReference)*/
                });

            var typeList = new List(new NonNull(type));

            var directiveLocation = new EnumType(
                "__DirectiveLocation",
                new EnumValues(Enum.GetNames(typeof(__DirectiveLocation))));

            var directive = new ObjectType(
                "__Directive",
                new Fields()
                {
                    ["name"] = new Field(ScalarType.String),
                    ["description"] = new Field(ScalarType.String),
                    ["locations"] = new Field(new List(directiveLocation)),
                    ["args"] = new Field(argsList),
                });

            var schema = new ObjectType(
                IntrospectionSchema.SchemaName,
                new Fields
                {
                    ["types"] = new Field(typeListReference),
                    ["queryType"] = new Field(type),
                    ["mutationType"] = new Field(type),
                    ["subscriptionType"] = new Field(type),
                    ["directives"] = new Field(new List(directive))
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    /*["__schema"] = new Field(schema),*/
                    ["__type"] = new Field(type, new Args
                    {
                        ["name"] = Arg(NonNullString)
                    })
                });

            /* When */
            var actual = Schema.Initialize(query);

            /* Then */
        }
    }
}