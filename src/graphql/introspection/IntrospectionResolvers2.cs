using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class IntrospectionResolvers2 : ResolverMap
    {
        public IntrospectionResolvers2(ISchema source)
        {
            this[source.Query.Name] = new FieldResolverMap()
            {
                {"__schema", context => SyncWrap.Sync(source)},
                /* {"__type", context => SyncWrap.Sync(translator.GetTypes(data))}*/
            };

            this[IntrospectionSchema.SchemaName] = new FieldResolverMap()
            {
                {"types", context => SyncWrap.Sync(source.QueryTypes<IType>())},
                {"queryType", context => SyncWrap.Sync(source.Query)},
                {"mutationType", context => SyncWrap.Sync(source.Mutation)},
                {"subscriptionType", context => SyncWrap.Sync(source.Subscription)},
                {"directives", context => SyncWrap.Sync(source.QueryDirectives())}
            };
        
            this[IntrospectionSchema.TypeName] = new FieldResolverMap()
            {
               /* {"kind", Resolve.PropertyOf<__Type>(t => t.Kind)},*/
                {"name", Resolve.PropertyOf<INamedType>(t => t.Name)},
                /*{"description", Resolve.PropertyOf<__Type>(t => t.Description)},
                {
                    "fields", context =>
                    {
                        var type = (__Type) context.ObjectValue;
                        var includeDeprecated = (bool) context.Arguments["includeDeprecated"];
                        return SyncWrap.Sync(type.GetFields(includeDeprecated));
                    }
                },
                {"interfaces", Resolve.PropertyOf<__Type>(t => translator.GetInterfacesOf(t, data))},
                {"possibleTypes", Resolve.PropertyOf<__Type>(t => translator.GetPossibleTypes(t.Name, data))},
                {
                    "enumValues", context =>
                    {
                        var type = (__Type) context.ObjectValue;
                        var includeDeprecated = (bool) context.Arguments["includeDeprecated"];
                        return SyncWrap.Sync(type.GetEnumValues(includeDeprecated));
                    }
                },
                {"inputFields", Resolve.PropertyOf<__Type>(t => t.InputFields)},
                {"ofType", Resolve.PropertyOf<__Type>(t => t.OfType)},*/
            };
            /*

            this[IntrospectionSchema.FieldName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__Field>(f => f.Name)},
                {"description", Resolve.PropertyOf<__Field>(f => f.Description)},
                {"args", Resolve.PropertyOf<__Field>(f => f.Args)},
                {"type", Resolve.PropertyOf<__Field>(f => translator.GetType(f.Type, data))},
                {"isDeprecated", Resolve.PropertyOf<__Field>(f => f.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<__Field>(f => f.DeprecationReason)},
            };

            this[IntrospectionSchema.InputValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__InputValue>(f => f.Name)},
                {"description", Resolve.PropertyOf<__InputValue>(f => f.Description)},
                {"type", Resolve.PropertyOf<__InputValue>(f => translator.GetType(f.Type, data))},
                {"defaultValue", Resolve.PropertyOf<__InputValue>(f => f.DefaultValue)}
            };

            this[IntrospectionSchema.EnumValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<__EnumValue>(f => f.Name)},
                {"description", Resolve.PropertyOf<__EnumValue>(f => f.Description)},
                {"isDeprecated", Resolve.PropertyOf<__EnumValue>(f => f.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<__EnumValue>(f => f.DeprecationReason)},
            };
            */
            this["__Directive"] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<DirectiveType>(d => d.Name)},
                {"description", Resolve.PropertyOf<DirectiveType>(d => d.Meta.Description)},
                /*{"locations", Resolve.PropertyOf<__Directive>(d => d.Locations)},
                {"args", Resolve.PropertyOf<__Directive>(d => d.Args)},*/
            };
            
        }
    }
}