using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Introspection
{
    public class IntrospectionResolvers : ResolversMap
    {
        public IntrospectionResolvers(ISchema source)
        {
            this[source.Query.Name] = new FieldResolversMap
            {
                {"__schema", context => ResolveSync.As(source)},
                {
                    "__type", context => ResolveSync.As(source.GetNamedType(context.GetArgument<string>("name")))
                }
            };

            this[IntrospectionSchema.SchemaName] = new FieldResolversMap
            {
                {"types", context => ResolveSync.As(source.QueryTypes<TypeDefinition>().OrderBy(t => t.Name.Value))},
                {"queryType", context => ResolveSync.As(source.Query)},
                {"mutationType", context => ResolveSync.As(source.Mutation)},
                {"subscriptionType", context => ResolveSync.As(source.Subscription)},
                {"directives", context => ResolveSync.As(source.QueryDirectiveTypes().OrderBy(t => t.Name.Value))}
            };

            this[IntrospectionSchema.TypeName] = new FieldResolversMap
            {
                {"kind", Resolve.PropertyOf<INode>(t => KindOf(source, t))},
                {"name", Resolve.PropertyOf<INode>(t => NameOf(source, t))},
                {"description", Resolve.PropertyOf<INode>(t => Describe(t))},

                // OBJECT and INTERFACE only
                {
                    "fields", context =>
                    {
                        var fields = context.ObjectValue switch
                        {
                            null => null,
                            ObjectDefinition objectDefinition => source.GetFields(objectDefinition.Name),
                            InterfaceDefinition interfaceDefinition => source.GetFields(interfaceDefinition.Name),
                            _ => null
                        };

                        if (fields is null)
                            return ResolveSync.As(null);

                        var includeDeprecated = context.GetArgument<bool?>("includeDeprecated") ?? false;
                        if (!includeDeprecated) 
                            fields = fields.Where(f => !f.Value.TryGetDirective("deprecated", out _));

                        return ResolveSync.As(fields.OrderBy(t => t.Key).ToList());
                    }
                },

                // OBJECT only
                {"interfaces", Resolve.PropertyOf<ObjectDefinition>(t => t.Interfaces?.OrderBy(t => t.Name.Value))},


                // INTERFACE and UNION only
                {
                    "possibleTypes", context =>
                    {
                        var possibleTypes = context.ObjectValue switch
                        {
                            null => null,
                            InterfaceDefinition interfaceDefinition => source.GetPossibleTypes(interfaceDefinition),
                            UnionDefinition unionDefinition => source.GetPossibleTypes(unionDefinition),
                            _ => null
                        };

                        return ResolveSync.As(possibleTypes?.OrderBy(t => t.Name.Value));
                    }
                },

                // ENUM only
                {
                    "enumValues", context =>
                    {
                        var en = context.ObjectValue as EnumDefinition;

                        if (en == null)
                            return ResolveSync.As(null);

                        var includeDeprecated = (bool?)context.GetArgument<bool?>("includeDeprecated") ?? false;

                        var values = en.Values?.ToList();

                        if (!includeDeprecated) 
                            values = values?.Where(f => !f.TryGetDirective("deprecated", out _)).ToList();

                        return ResolveSync.As(values?.OrderBy(t => t.Value.Name.Value));
                    }
                },

                // INPUT_OBJECT only
                {
                    "inputFields", Resolve.PropertyOf<InputObjectDefinition>(t => source.GetInputFields(t.Name)
                        .Select(iof => iof.Value).OrderBy(t => t.Name.Value).ToList())
                },

                // NON_NULL and LIST only
                {"ofType", Resolve.PropertyOf<TypeBase>(t => t switch
                {
                    null => null,
                    NonNullType nonNullType => nonNullType.OfType,
                    ListType list => list.OfType,
                    _ => null
                })}
            };

            this[IntrospectionSchema.FieldName] = new FieldResolversMap
            {
                {"name", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Key)},
                {"description", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.Description)},
                {"args", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.Arguments ?? ArgumentsDefinition.None)},
                {"type", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.Type)},
                {"isDeprecated", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.IsDeprecated(out _))},
                {"deprecationReason", Resolve.PropertyOf<KeyValuePair<string, FieldDefinition>>(f => f.Value.IsDeprecated(out var reason) ? reason: null)}
            };

            this[IntrospectionSchema.InputValueName] = new FieldResolversMap
            {
                {"name", Resolve.PropertyOf<InputValueDefinition>(f => f.Name.Value)},
                {"description", Resolve.PropertyOf<InputValueDefinition>(f => f.Description)},
                {"type", Resolve.PropertyOf<InputValueDefinition>(f => f.Type)},
                {"defaultValue", Resolve.PropertyOf<InputValueDefinition>(f => Execution.Values.CoerceValue(source, f.DefaultValue?.Value, f.Type))}
            };

            this[IntrospectionSchema.EnumValueName] = new FieldResolversMap
            {
                {"name", Resolve.PropertyOf<EnumValueDefinition>(f => f.Value.Name)},
                {"description", Resolve.PropertyOf<EnumValueDefinition>(f => f.Description)},
                {"isDeprecated", Resolve.PropertyOf<EnumValueDefinition>(f => f.IsDeprecated(out _))},
                {
                    "deprecationReason", Resolve.PropertyOf<EnumValueDefinition>(f => f.IsDeprecated(out var reason) ? reason : null)
                }
            };

            this["__Directive"] = new FieldResolversMap
            {
                {"name", Resolve.PropertyOf<DirectiveDefinition>(d => d.Name)},
                {"description", Resolve.PropertyOf<DirectiveDefinition>(d => d.Description)},
                {"locations", Resolve.PropertyOf<DirectiveDefinition>(d => LocationsOf(d.DirectiveLocations))},
                {"args", Resolve.PropertyOf<DirectiveDefinition>(d =>
                {
                    return d.Arguments?.OrderBy(t => t.Name.Value);
                })}
            };
        }

        private string? NameOf(ISchema source, INode? type)
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

        public static __TypeKind KindOf(ISchema source, INode type)
        {
            return type switch
            {
                NamedType namedType => KindOf(source, source.GetRequiredNamedType<TypeDefinition>(namedType.Name)),
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

        private IEnumerable<__DirectiveLocation> LocationsOf(IEnumerable<string> locations)
        {
            return locations
                .Select(l => (__DirectiveLocation) Enum.Parse(
                    typeof(__DirectiveLocation),
                    l.ToString())).ToList();
        }
    }
}