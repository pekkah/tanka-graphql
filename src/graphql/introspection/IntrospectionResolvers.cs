using System.Collections.Generic;
using fugu.graphql.resolvers;
using fugu.graphql.type;

namespace fugu.graphql.introspection
{
    public class IntrospectionResolvers : ResolverMap
    {
        public IntrospectionResolvers(__Schema translator, ISchema data)
        {
            this[data.Query.Name] = new FieldResolverMap()
            {
                {"__schema", context => SyncWrap.Sync(translator)},
                {"__type", context => SyncWrap.Sync(translator.GetTypes(data))}
            };

            this[IntrospectionSchemaBuilder.SchemaName] = new FieldResolverMap()
            {
                {"types", context => SyncWrap.Sync(translator.GetTypes(data))},
                {"queryType", context => SyncWrap.Sync(translator.GetType("Query", data))},
                {"mutationType", context => SyncWrap.Sync(translator.GetType("Mutation", data))},
                {"subscriptionType", context => SyncWrap.Sync(translator.GetType("Subscription", data))},
                {"directives", context => SyncWrap.Sync(translator.GetDirectives(data))}
            };

            this[IntrospectionSchemaBuilder.TypeName] = new FieldResolverMap()
            {
                {"kind", Resolve.PropertyOf<__Type>(t => t.Kind)},
                {"name", Resolve.PropertyOf<__Type>(t => t.Name)},
                {"description", Resolve.PropertyOf<__Type>(t => t.Description)},
                {"fields", context =>
                {
                    var type = (__Type) context.ObjectValue;
                    var includeDeprecated = (bool) context.Arguments["includeDeprecated"];
                    return SyncWrap.Sync(type.GetFields(includeDeprecated));
                }},
                {"interfaces", Resolve.PropertyOf<__Type>(t => translator.GetInterfacesOf(t, data))},
                {"possibleTypes", Resolve.PropertyOf<__Type>(t => translator.GetPossibleTypes(t.Name, data))},
                {"enumValues", context =>
                {
                    var type = (__Type) context.ObjectValue;
                    var includeDeprecated = (bool) context.Arguments["includeDeprecated"];
                    return SyncWrap.Sync(type.GetEnumValues(includeDeprecated));
                }},
                {"inputFields", Resolve.PropertyOf<__Type>(t => t.InputFields)},
                {"ofType", Resolve.PropertyOf<__Type>(t => t.OfType)},
            };

            this[IntrospectionSchemaBuilder.FieldName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__Field>(f => f.Name)},
                {"description", Resolve.PropertyOf<__Field>(f => f.Description)},
                {"args", Resolve.PropertyOf<__Field>(f => f.Args)},
                {"type", Resolve.PropertyOf<__Field>(f => translator.GetType(f.Type, data))},
                {"isDeprecated", Resolve.PropertyOf<__Field>(f => f.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<__Field>(f => f.DeprecationReason)},
            };

            this[IntrospectionSchemaBuilder.InputValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__InputValue>(f => f.Name)},
                {"description", Resolve.PropertyOf<__InputValue>(f => f.Description)},
                {"type", Resolve.PropertyOf<__InputValue>(f => translator.GetType(f.Type, data))},
                {"defaultValue", Resolve.PropertyOf<__InputValue>(f => f.DefaultValue)}
            };

            this[IntrospectionSchemaBuilder.EnumValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__EnumValue>(f => f.Name)},
                {"description", Resolve.PropertyOf<__EnumValue>(f => f.Description)},
                {"isDeprecated", Resolve.PropertyOf<__EnumValue>(f => f.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<__EnumValue>(f => f.DeprecationReason)},
            };

            this["__Directive"] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__Directive>(d => d.Name)},
                {"description", Resolve.PropertyOf<__Directive>(d => d.Description)},
                {"locations", Resolve.PropertyOf<__Directive>(d => d.Locations)},
                {"args", Resolve.PropertyOf<__Directive>(d => d.Args)},
            };
        }
    }
}