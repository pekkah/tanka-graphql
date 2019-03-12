using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.resolvers;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class IntrospectionResolvers : ResolverMap
    {
        public IntrospectionResolvers(ISchema source)
        {
            this[source.Query.Name] = new FieldResolverMap()
            {
                {"__schema", context => SyncWrap.Sync(source)},
                {"__type", context => SyncWrap.Sync(
                    source.GetNamedType(context.GetArgument<string>("name")))}
            };

            this[IntrospectionSchema.SchemaName] = new FieldResolverMap()
            {
                {"types", context => SyncWrap.Sync(source.QueryTypes<INamedType>())},
                {"queryType", context => SyncWrap.Sync(source.Query)},
                {"mutationType", context => SyncWrap.Sync(source.Mutation)},
                {"subscriptionType", context => SyncWrap.Sync(source.Subscription)},
                {"directives", context => SyncWrap.Sync(source.QueryDirectiveTypes())}
            };
        
            this[IntrospectionSchema.TypeName] = new FieldResolverMap()
            {
                {"kind", Resolve.PropertyOf<IType>(t => KindOf(t))},
                {"name", Resolve.PropertyOf<INamedType>(t => t.Name)},
                {"description", Resolve.PropertyOf<IDescribable>(t => t.Description)},
                
                // OBJECT and INTERFACE only
                {"fields", context =>
                    {
                        if (!(context.ObjectValue is ComplexType complexType))
                            return SyncWrap.Sync(null);

                        var includeDeprecated = (bool) context.Arguments["includeDeprecated"];

                        var fields = source.GetFields(complexType.Name);

                        if (!includeDeprecated)
                        {
                            fields = fields.Where(f => !f.Value.IsDeprecated);
                        }

                        return SyncWrap.Sync(fields.ToList());
                    }
                },
                
                // OBJECT only
                {"interfaces", Resolve.PropertyOf<ObjectType>(t => t.Interfaces)},
                
                
                // INTERFACE and UNION only
                {"possibleTypes", context =>
                {
                    List<ObjectType> possibleTypes = null;

                    switch (context.ObjectValue)
                    {
                        case InterfaceType interfaceType:
                        {
                            var objects = source.QueryTypes<ObjectType>()
                                .Where(o => o.Implements(interfaceType))
                                .ToList();

                            possibleTypes = objects;
                            break;
                        }
                        case UnionType unionType:
                            possibleTypes = unionType.PossibleTypes.Select(p => p.Value)
                                .ToList();
                            break;
                    }

                    return SyncWrap.Sync(possibleTypes);
                }},
                
                // ENUM only
                {"enumValues", context =>
                    {
                        var en = context.ObjectValue as EnumType;

                        if (en == null)
                            return SyncWrap.Sync(null);

                        var includeDeprecated = (bool) context.Arguments["includeDeprecated"];

                        var values = en.Values;

                        if (!includeDeprecated)
                        {
                            values = values.Where(v => !v.Value.IsDeprecated);
                        }
                        return SyncWrap.Sync(values);
                    }
                },
                
                // INPUT_OBJECT only
                //todo(pekka): DirectiveType uses Argument but should use InputFieldType
                {"inputFields", Resolve.PropertyOf<InputObjectType>(t => source.GetInputFields(t.Name)
                    .Select(iof => new KeyValuePair<string, Argument>(
                        iof.Key, 
                        new Argument(iof.Value.Type, iof.Value.DefaultValue, iof.Value.Description))).ToList())},

                // NON_NULL and LIST only
                {"ofType", Resolve.PropertyOf<IWrappingType>(t => t.WrappedType)},
            };
            
            this[IntrospectionSchema.FieldName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Key)},
                {"description", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Value.Description)},
                {"args", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Value.Arguments)},
                {"type", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Value.Type)},
                {"isDeprecated", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Value.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<KeyValuePair<string, IField>>(f => f.Value.DeprecationReason)},
            };

            this[IntrospectionSchema.InputValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<KeyValuePair<string, Argument>>(f => f.Key)},
                {"description", Resolve.PropertyOf<KeyValuePair<string, Argument>>(f => f.Value.Description)},
                {"type", Resolve.PropertyOf<KeyValuePair<string, Argument>>(f => f.Value.Type)},
                {"defaultValue", Resolve.PropertyOf<KeyValuePair<string, Argument>>(f => f.Value.DefaultValue)}
            };
            
            this[IntrospectionSchema.EnumValueName] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<KeyValuePair<string, EnumValue>>(f => f.Key)},
                {"description", Resolve.PropertyOf<KeyValuePair<string, EnumValue>>(f => f.Value.Description)},
                {"isDeprecated", Resolve.PropertyOf<KeyValuePair<string, EnumValue>>(f => f.Value.IsDeprecated)},
                {"deprecationReason", Resolve.PropertyOf<KeyValuePair<string, EnumValue>>(f => f.Value.DeprecationReason)},
            };
        
            this["__Directive"] = new FieldResolverMap()
            {
                {"name", Resolve.PropertyOf<DirectiveType>(d => d.Name)},
                {"description", Resolve.PropertyOf<DirectiveType>(d => d.Description)},
                {"locations", Resolve.PropertyOf<DirectiveType>(d => LocationsOf(d.Locations))},
                {"args", Resolve.PropertyOf<DirectiveType>(d => d.Arguments)},
            };        
        }

        private IEnumerable<__DirectiveLocation> LocationsOf(IEnumerable<DirectiveLocation> locations)
        {
            return locations
                .Select(l => (__DirectiveLocation) Enum.Parse(
                    typeof(__DirectiveLocation),
                    l.ToString())).ToList();
        }

        public static __TypeKind KindOf(IType type)
        {
            switch (type)
            {
                case ObjectType _:
                    return __TypeKind.OBJECT;
                case ScalarType _:
                    return __TypeKind.SCALAR;
                case EnumType _:
                    return __TypeKind.ENUM;
               case InputObjectType _:
                   return __TypeKind.INPUT_OBJECT;
               case InterfaceType _:
                   return __TypeKind.INTERFACE;
               case List _:
                   return __TypeKind.LIST;
               case NonNull _:
                   return __TypeKind.NON_NULL;
               case UnionType _:
                   return __TypeKind.UNION;
               default:
                   throw new InvalidOperationException($"Cannot get kind form {type}");
            }
        }
    }
}