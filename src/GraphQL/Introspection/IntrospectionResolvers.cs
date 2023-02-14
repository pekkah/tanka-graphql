using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Introspection;

public class IntrospectionResolvers : ResolversMap
{
    public IntrospectionResolvers(string queryTypeName = "Query")
    {
        this[queryTypeName] = new()
        {
            { "__schema", context => context.ResolveAs(context.Schema) },
            {
                "__type",
                context => context.ResolveAs(context.Schema.GetNamedType(
                    context.GetArgument<string>("name") ?? throw new ArgumentNullException("name")))
            }
        };

        this[IntrospectionSchema.SchemaName] = new()
        {
            { "description", context => context.ResolveAsPropertyOf<ISchema>(schema => schema.Description) },
            {
                "types", context => context.ResolveAs(context.Schema
                    .QueryTypes<TypeDefinition>(IsNotBuiltIn)
                    .OrderBy(t => t.Name.Value))
            },
            { "queryType", context => context.ResolveAs(context.Schema.Query) },
            { "mutationType", context => context.ResolveAs(context.Schema.Mutation) },
            { "subscriptionType", context => context.ResolveAs(context.Schema.Subscription) },
            { "directives", context => context.ResolveAs(context.Schema.QueryDirectiveTypes().OrderBy(t => t.Name.Value)) }
        };

        this[IntrospectionSchema.TypeName] = new()
        {
            { "kind", context => context.ResolveAsPropertyOf<INode>(t => KindOf(context.Schema, t)) },
            { "name", context => context.ResolveAsPropertyOf<INode>(t => NameOf(context.Schema, t)) },
            { "description", context => context.ResolveAsPropertyOf<INode>(Describe) },

            // OBJECT and INTERFACE only
            {
                "fields", context =>
                {
                    var fields = context.ObjectValue switch
                    {
                        null => null,
                        ObjectDefinition objectDefinition => context.Schema.GetFields(objectDefinition.Name),
                        InterfaceDefinition interfaceDefinition => context.Schema.GetFields(interfaceDefinition.Name),
                        _ => null
                    };

                    if (fields is null)
                    {
                        context.ResolvedValue = null;
                        return default;
                    }

                    var includeDeprecated = context.GetArgument<bool?>("includeDeprecated") ?? false;
                    if (!includeDeprecated)
                        fields = fields.Where(f => !f.Value.TryGetDirective("deprecated", out _));


                    return context.ResolveAs(fields
                        .Where(f => !f.Key.StartsWith("__"))
                        .OrderBy(t => t.Key).ToList());
                }
            },

            // OBJECT only
            {
                "interfaces", context => context.ResolveAsPropertyOf<INode>(t =>
                {
                    var interfaces = t switch
                    {
                        null => null,
                        InterfaceDefinition interfaceDefinition => interfaceDefinition.Interfaces ??
                                                                   ImplementsInterfaces.None,
                        ObjectDefinition objectDefinition => objectDefinition.Interfaces ?? ImplementsInterfaces.None,
                        _ => null
                    };

                    if (interfaces is null)
                        return null;

                    var interfaceNames = interfaces.Select(i => i.Name.Value).ToList();

                    // objects and interfaces must return non null value
                    var interfaceTypeDefinitions = context.Schema
                        .QueryTypes<InterfaceDefinition>(i => interfaceNames.Contains(i.Name.Value))
                        .ToList();

                    return interfaceTypeDefinitions;
                })
            },


            // INTERFACE and UNION only
            {
                "possibleTypes", context =>
                {
                    var possibleTypes = context.ObjectValue switch
                    {
                        null => null,
                        InterfaceDefinition interfaceDefinition => context.Schema.GetPossibleTypes(interfaceDefinition),
                        UnionDefinition unionDefinition => context.Schema.GetPossibleTypes(unionDefinition),
                        _ => null
                    };

                    return context.ResolveAs(possibleTypes?.OrderBy(t => t.Name.Value));
                }
            },

            // ENUM only
            {
                "enumValues", context =>
                {
                    var en = context.ObjectValue as EnumDefinition;

                    if (en == null)
                    {
                        return default;
                    }

                    var includeDeprecated = context.GetArgument<bool?>("includeDeprecated") ?? false;

                    var values = en.Values?.ToList();

                    if (!includeDeprecated)
                        values = values?.Where(f => !f.TryGetDirective("deprecated", out _)).ToList();

                    return context.ResolveAs(values?.OrderBy(t => t.Value.Name.Value));
                }
            },

            // INPUT_OBJECT only
            {
                "inputFields", context => context.ResolveAsPropertyOf<InputObjectDefinition>(t => context.Schema
                    .GetInputFields(t.Name)
                    .Select(iof => iof.Value).OrderBy(t => t.Name.Value).ToList())
            },

            // NON_NULL and LIST only
            {
                "ofType", context => context.ResolveAsPropertyOf<TypeBase>(t => t switch
                {
                    null => null,
                    NonNullType nonNullType => nonNullType.OfType,
                    ListType list => list.OfType,
                    _ => null
                })
            }
        };

        this[IntrospectionSchema.FieldName] = new()
        {
            { "name", context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Key) },
            { "description", context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.Description) },
            {
                "args",
                context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f =>
                    f.Value.Arguments ?? ArgumentsDefinition.None)
            },
            { "type", context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.Type) },
            {
                "isDeprecated",
                context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.IsDeprecated(out _))
            },
            {
                "deprecationReason",
                context => context.ResolveAsPropertyOf<KeyValuePair<string, FieldDefinition>>(f =>
                    f.Value.IsDeprecated(out var reason) ? reason : null)
            }
        };

        this[IntrospectionSchema.InputValueName] = new()
        {
            { "name", context => context.ResolveAsPropertyOf<InputValueDefinition>(f => f.Name.Value) },
            { "description", context => context.ResolveAsPropertyOf<InputValueDefinition>(f => f.Description) },
            { "type", context => context.ResolveAsPropertyOf<InputValueDefinition>(f => f.Type) },
            {
                "defaultValue", context => context.ResolveAsPropertyOf<InputValueDefinition>(f =>
                {
                    try
                    {
                        return GraphQL.Values.CoerceValue(context.Schema, f.DefaultValue?.Value, f.Type);
                    }
                    catch
                    {
                        return null;
                    }
                })
            }
        };

        this[IntrospectionSchema.EnumValueName] = new()
        {
            { "name", context => context.ResolveAsPropertyOf<EnumValueDefinition>(f => f.Value.Name) },
            { "description", context => context.ResolveAsPropertyOf<EnumValueDefinition>(f => f.Description) },
            { "isDeprecated", context => context.ResolveAsPropertyOf<EnumValueDefinition>(f => f.IsDeprecated(out _)) },
            {
                "deprecationReason",
                context => context.ResolveAsPropertyOf<EnumValueDefinition>(f => f.IsDeprecated(out var reason) ? reason : null)
            }
        };

        this["__Directive"] = new()
        {
            { "name", context => context.ResolveAsPropertyOf<DirectiveDefinition>(d => d.Name) },
            { "description", context => context.ResolveAsPropertyOf<DirectiveDefinition>(d => d.Description) },
            { "locations", context => context.ResolveAsPropertyOf<DirectiveDefinition>(d => LocationsOf(d.DirectiveLocations)) },
            {
                "args",
                context => context.ResolveAsPropertyOf<DirectiveDefinition>(d =>
                {
                    return d.Arguments?.OrderBy(t => t.Name.Value)
                        ?? Enumerable.Empty<InputValueDefinition>();
                })
            },
            { "isRepeatable", context => context.ResolveAsPropertyOf<DirectiveDefinition>(d => d.IsRepeatable) }
        };
    }

    private IReadOnlyList<string> BuiltInTypes => new List<string>
    {
        "__Directive",
        "__EnumValue",
        "__Field",
        "__InputValue",
        "__Schema",
        "__Type",
        "__TypeKind",
        "__DirectiveLocation"
    };

    public static __TypeKind KindOf(ISchema schema, INode type)
    {
        return type switch
        {
            NamedType namedType => KindOf(schema, schema.GetRequiredNamedType<TypeDefinition>(namedType.Name)),
            ObjectDefinition _ => __TypeKind.OBJECT,
            ScalarDefinition _ => __TypeKind.SCALAR,
            EnumDefinition _ => __TypeKind.ENUM,
            InputObjectDefinition _ => __TypeKind.INPUT_OBJECT,
            InterfaceDefinition _ => __TypeKind.INTERFACE,
            ListType _ => __TypeKind.LIST,
            NonNullType _ => __TypeKind.NON_NULL,
            UnionDefinition _ => __TypeKind.UNION,
            _ => throw new InvalidOperationException($"Cannot get kind from {type}")
        };
    }

    private bool IsNotBuiltIn(TypeDefinition maybeBuiltIn)
    {
        var builtInIntrospectionType = BuiltInTypes.Contains(maybeBuiltIn.Name.Value);
        return !builtInIntrospectionType;
    }


    private string? NameOf(ISchema context, INode? type)
    {
        return type switch
        {
            null => null,
            NamedType namedType => namedType.Name.Value,
            TypeDefinition typeDefinition => typeDefinition.Name.Value,
            _ => null
        };
    }

    private string? Describe(INode node)
    {
        return node switch
        {
            null => null,
            ObjectDefinition objectDefinition => objectDefinition.Description,
            InterfaceDefinition interfaceDefinition => interfaceDefinition.Description,
            FieldDefinition fieldDefinition => fieldDefinition.Description,
            DirectiveDefinition directiveDefinition => directiveDefinition.Description,
            ScalarDefinition scalarDefinition => scalarDefinition.Description,
            UnionDefinition unionDefinition => unionDefinition.Description,
            EnumDefinition enumDefinition => enumDefinition.Description,
            InputObjectDefinition inputObjectDefinition => inputObjectDefinition.Description,
            InputValueDefinition inputValueDefinition => inputValueDefinition.Description,
            EnumValueDefinition enumValueDefinition => enumValueDefinition.Description
        };
    }

    private IEnumerable<__DirectiveLocation> LocationsOf(IEnumerable<string> locations)
    {
        return locations
            .Select(l => (__DirectiveLocation)Enum.Parse(
                typeof(__DirectiveLocation),
                l.ToString())).ToList();
    }
}