using System;
using System.Collections.Generic;
using tanka.graphql.type;
using static tanka.graphql.graph.Wrapper;

namespace tanka.graphql.graph
{
    public delegate ISchema SchemaTransform(ISchema schema);

    public static class Transforms
    {
        public static ISchema Apply(ISchema schema, params SchemaTransform[] transforms)
        {
            var result = schema;
            foreach (var transform in transforms) 
                result = transform(schema);

            return result;
        }

        public static SchemaTransform Heal()
        {
            var healedTypes = new Dictionary<string, IType>();

            var healingTypes = new List<string>();

            IType HealType(IType type, Func<string, INamedType> getNamedType)
            {
                var namedType = type as INamedType;

                if (namedType == null)
                    return type;

                if (healedTypes.ContainsKey(namedType.Name))
                    return healedTypes[namedType.Name];

                healingTypes.Add(namedType.Name);

                var newType = namedType;
                if (namedType is ObjectType objectType)
                    newType = objectType.WithEachField(
                            field => HealField(objectType, field, getNamedType)
                        )
                        .WithEachInterface(inf => (InterfaceType) HealType(inf, getNamedType));

                if (namedType is InterfaceType interfaceType)
                    newType = interfaceType.WithEachField(
                        field => HealField(interfaceType, field, getNamedType)
                    );

                if (namedType is UnionType unionType)
                    newType = unionType.WithEachPossibleType(possibleType =>
                    {
                        var newPossibleType = (ObjectType) HealType(possibleType, getNamedType);
                        return newPossibleType;
                    });

                healedTypes.Add(newType.Name, newType);
                return newType;
            }

            KeyValuePair<string, IField> HealField(
                ComplexType complexType,
                KeyValuePair<string, IField> field,
                Func<string, INamedType> getNamedType)
            {
                var name = field.Key;
                var value = field.Value;
                var unwrappedFieldType = value.Type.Unwrap();

                if (unwrappedFieldType is NamedTypeReference typeReference)
                {
                    var namedType = getNamedType(typeReference.TypeName);

                    if (namedType == null)
                        throw new InvalidOperationException(
                            "Failed to heal schema. Could not build named type for field " +
                            $"{complexType}.{name} from reference " +
                            $"{typeReference.TypeName}");

                    if (Equals(complexType, namedType))
                        return field.WithType(
                            WrapIfRequired(value.Type, 
                                new SelfReferenceType()));

                    return field.WithType(
                        WrapIfRequired(value.Type,
                        HealType(namedType, getNamedType)));
                }

                return field.WithType(WrapIfRequired(
                    value.Type,
                    HealType(unwrappedFieldType, getNamedType)));
            }

            return schema => schema.WithRoots((roots, getNamedType) =>
            {
                var (query, mutation, subscription) = roots;

                var newQuery = (ObjectType)HealType(query, getNamedType);

                var newMutation = (ObjectType)HealType(mutation, getNamedType);

                var newSubscription = (ObjectType)HealType(subscription, getNamedType);

                return (newQuery, newMutation, newSubscription);
            });
        }

        public static SchemaTransform Delete(string name)
        {
            IType Delete(INamedType deleteMe, IType source)
            {
                if (source == null)
                    return null;

                if (Equals(source, deleteMe))
                    return null;

                if (source is ObjectType objectType)
                {
                    return objectType.WithEachField(field =>
                    {
                        var fieldType = field.Value.Type.Unwrap();

                        if (Equals(fieldType, deleteMe))
                            return default(KeyValuePair<string, IField>);

                        if (fieldType is ComplexType complexType)
                            return field.WithType(Delete(deleteMe, complexType));

                        return field;
                    });
                }

                if (source is InterfaceType interfaceType)
                {
                    return interfaceType.WithEachField(field =>
                    {
                        var fieldType = field.Value.Type.Unwrap();

                        if (Equals(fieldType, deleteMe))
                            return default;

                        if (fieldType is ComplexType complexType)
                            return field.WithType(Delete(deleteMe, complexType));

                        return field;
                    });
                }

                if (source is UnionType unionType)
                {
                    return unionType.WithEachPossibleType(possibleType =>
                    {
                        if (Equals(possibleType, deleteMe))
                            return null;

                        return possibleType;
                    });
                }

                return source;
            }

            return schema =>
            {
                var matchingType = schema.GetNamedType(name);

                return schema.WithRoots((roots, getNamedType) =>
                {
                    var (query, mutation, subscription) = roots;

                    var newQuery = (ObjectType)Delete(matchingType, query);
                    var newMutation = (ObjectType)Delete(matchingType, mutation);
                    var newSubscription = (ObjectType)Delete(matchingType, subscription);

                    return (newQuery, newMutation, newSubscription);
                });
            };
        }
    }
}