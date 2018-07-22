using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.type;

namespace fugu.graphql.tools
{
    public abstract class SchemaVisitorBase
    {
        protected IResolverMap ResolverMap { get; }
        protected ISchema Schema { get; }
        protected ISubscriberMap SubscriberMap { get; }

        protected SchemaVisitorBase(
            ISchema schema,
            IResolverMap resolverMap,
            ISubscriberMap subscriberMap)
        {
            Schema = schema;
            ResolverMap = resolverMap;
            SubscriberMap = subscriberMap;
        }

        public virtual async Task VisitAsync()
        {
            foreach (var directiveType in Schema.QueryDirectives())
            {
                foreach (var argument in directiveType.Arguments) await VisitArgumentAsync(directiveType, argument);

                await VisitDirectiveAsync(directiveType);
            }

            foreach (var scalarType in Schema.QueryTypes<ScalarType>()) await VisitScalarAsync(scalarType);

            foreach (var enumType in Schema.QueryTypes<EnumType>()) await VisitEnumAsync(enumType);

            foreach (var inputObjectType in Schema.QueryTypes<InputObjectType>())
            {
                await VisitInputObjectTypeAsync(inputObjectType);

                foreach (var inputObjectField in inputObjectType.Fields)
                    await VisitInputObjectFieldAsync(inputObjectType, inputObjectField);
            }

            foreach (var interfaceType in Schema.QueryTypes<InterfaceType>())
            {
                await VisitInterfaceTypeAsync(interfaceType);

                foreach (var interfaceTypeField in interfaceType.Fields)
                {
                    foreach (var argument in interfaceTypeField.Value.Arguments)
                        await VisitArgumentAsync(interfaceType, interfaceTypeField, argument);

                    await VisitInterfaceFieldAsync(interfaceType, interfaceTypeField);
                }
            }

            foreach (var objectType in Schema.QueryTypes<ObjectType>())
            {
                await VisitObjectTypeAsync(objectType);

                foreach (var objectTypeField in objectType.Fields)
                {
                    foreach (var argument in objectTypeField.Value.Arguments)
                        await VisitArgumentAsync(objectType, objectTypeField, argument);

                    await VisitObjectFieldAsync(objectType, objectTypeField);
                }
            }

            foreach (var unionType in Schema.QueryTypes<UnionType>()) await VisitUnionTypeAsync(unionType);
        }

        protected virtual Task VisitUnionTypeAsync(UnionType unionType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitObjectFieldAsync(ObjectType objectType,
            KeyValuePair<string, IField> objectTypeField)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitObjectTypeAsync(ObjectType objectType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitArgumentAsync(DirectiveType directiveType, KeyValuePair<string, Argument> argument)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitArgumentAsync(ComplexType complexType, KeyValuePair<string, IField> field,
            KeyValuePair<string, Argument> argument)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitDirectiveAsync(DirectiveType directiveType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitEnumAsync(EnumType enumType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitInterfaceFieldAsync(InterfaceType interfaceType,
            KeyValuePair<string, IField> interfaceTypeField)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitInterfaceTypeAsync(InterfaceType interfaceType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitInputObjectFieldAsync(InputObjectType inputObjectType,
            KeyValuePair<string, IField> inputObjectField)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitInputObjectTypeAsync(InputObjectType inputObjectType)
        {
            return Task.CompletedTask;
        }

        protected virtual Task VisitScalarAsync(ScalarType scalarType)
        {
            return Task.CompletedTask;
        }
    }
}