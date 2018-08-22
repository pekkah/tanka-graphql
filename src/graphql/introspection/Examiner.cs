using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.type;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace fugu.graphql.introspection
{
    public static class Examiner
    {
        private static readonly ConcurrentDictionary<string, __Type> _cache 
            = new ConcurrentDictionary<string, __Type>();

        public static __Schema Examine(ISchema schema)
        {
            return new __Schema()
            {

            };
        }

        public static __Type Examine(IGraphQLType type, ISchema schema)
        {
            if (type == null)
                return null;
            
            if (!string.IsNullOrEmpty(type.Name))
            {
                return _cache.GetOrAdd(type.Name, name => InternalExamine(type, schema));
            }

            return InternalExamine(type, schema);
        }

        public static __Type InternalExamine(IGraphQLType type, ISchema schema)
        {
            switch (type)
            {
                case NamedTypeReference typeRef:
                    return Examine(schema.GetNamedType(typeRef.TypeName), schema);
                case List listType:
                    return ExamineList(listType, schema);
                case NonNull nonNull:
                    return ExamineNonNull(nonNull, schema);
                case ScalarType scalarType:
                    return ExamineScalar(scalarType, schema);
                case ObjectType objectType:
                    return ExamineObject(objectType, schema);
                case UnionType unionType:
                    return ExamineUnion(unionType, schema);
                case InterfaceType interfaceType:
                    return ExamineInterface(interfaceType, schema);
                case EnumType enumType:
                    return ExamineEnum(enumType, schema);
                case InputObjectType inputObjectType:
                    return ExamineInput(inputObjectType, schema);
                default:
                    throw new InvalidOperationException(
                        $"Cannot examine type '{type}' as it's unknown type");
            }
        }

        public static __Directive ExamineDirective(DirectiveType directiveType, ISchema schema)
        {
            return new __Directive()
            {
                Name = directiveType.Name,
                Description = directiveType.Meta.Description,
                Args = directiveType.Arguments.Select(a => ExamineArg(a, schema)).ToList(),
                Locations = directiveType.Locations.Select(l => (__DirectiveLocation)Enum.Parse(typeof(__DirectiveLocation), l.ToString())).ToList()
            };
        }

        private static __Type ExamineNonNull(
            NonNull nonNull, 
            ISchema schema)
        {
            return new __Type
            {
                Kind = __TypeKind.NON_NULL,
                OfType = Examine(nonNull.WrappedType, schema)
            };
        }

        private static IGraphQLType BuildTypeRef(IGraphQLType type)
        {
            if (type is NonNull nonNull)
            {
                return new NonNull(BuildTypeRef(nonNull.WrappedType));
            }

            if (type is List list)
            {
                return new List(BuildTypeRef(list.WrappedType));
            }

            if (type is NamedTypeReference)
            {
                return type;
            }

            if (type is EnumType)
                return type;

            if (type is ScalarType)
                return type;

            if (string.IsNullOrEmpty(type.Name))
                throw new InvalidOperationException(
                    $"Cannot build named type reference from {type} as it doesn't have name");

            return new NamedTypeReference(type.Name);
        }

        private static __Type ExamineList(
            List listType, 
            ISchema schema)
        {
            return new __Type
            {
                Kind = __TypeKind.LIST,
                OfType = Examine(listType.WrappedType, schema)
            };
        }

        private static __Type ExamineInput(
            InputObjectType inputObjectType, 
            ISchema schema)
        {
            var fields = inputObjectType.Fields
                .Select(f => ExamineInputField(f, schema));

            return new __Type
            {
                Kind = __TypeKind.INPUT_OBJECT,
                Name = inputObjectType.Name,
                Description = inputObjectType.Meta.Description,
                InputFields = fields.ToList()
            };
        }

        private static __InputValue ExamineInputField(
            KeyValuePair<string, InputObjectField> field,
            ISchema schema)
        {
            return new __InputValue
            {
                Name = field.Key,
                Description = field.Value.Meta.Description,
                Type = BuildTypeRef(field.Value.Type),
                DefaultValue = field.Value.DefaultValue != null ? JsonConvert.SerializeObject(field.Value.DefaultValue):null
            };
        }

        private static __Type ExamineEnum(
            EnumType enumType,
            ISchema schema)
        {
            var enumValues = enumType.Values.Select(v => new __EnumValue
            {
                Name = v.Key,
                Description = v.Value.Description,
                IsDeprecated = v.Value.IsDeprecated,
                DeprecationReason = v.Value.DeprecationReason
            });

            return new __Type
            {
                Kind = __TypeKind.ENUM,
                Name = enumType.Name,
                Description = enumType.Meta.Description,
                EnumValues = enumValues.ToList()
            };
        }

        private static __Type ExamineUnion(
            UnionType unionType, 
            ISchema schema)
        {
            return new __Type
            {
                Kind = __TypeKind.UNION,
                Name = unionType.Name,
                Description = unionType.Meta.Description
            };
        }

        public static __Type ExamineScalar(ScalarType scalarType, ISchema schema)
        {
            return new __Type
            {
                Kind = __TypeKind.SCALAR,
                Name = scalarType.Name,
                Description = scalarType.Meta.Description
            };
        }

        public static __Type ExamineObject(
            ObjectType objectType, 
            ISchema schema)
        {
            var fields = objectType.Fields
                .Select(f => ExamineField(f, schema))
                .ToList();

            var interfaces = objectType.Interfaces
                .Select(i => i.Name)
                .ToList();

            var type = new __Type
            {
                Kind = __TypeKind.OBJECT,
                Name = objectType.Name,
                Description = objectType.Meta.Description,
                Fields = fields.ToList(),
                Interfaces = interfaces
            };

            return type;
        }

        private static __Type ExamineInterface(
            InterfaceType interfaceType,
            ISchema schema)
        {
            var fields = interfaceType.Fields.Select(f => ExamineField(f, schema)).ToList();

            return new __Type
            {
                Kind = __TypeKind.INTERFACE,
                Name = interfaceType.Name,
                Description = interfaceType.Meta.Description,
                Fields = fields.ToList()
            };
        }

        private static __Field ExamineField(
            KeyValuePair<string, IField> field,
            ISchema schema)
        {
            var f = new __Field
            {
                Name = field.Key,
                Description = field.Value.Meta.Description,
                Args = field.Value.Arguments.Select(a => ExamineArg(a, schema)).ToList(),
                Type = BuildTypeRef(field.Value.Type),
                IsDeprecated = field.Value.Meta.IsDeprecated,
                DeprecationReason = field.Value.Meta.DeprecationReason
            };

            return f;
        }

        private static __InputValue ExamineArg(
            KeyValuePair<string, Argument> arg,
            ISchema schema)
        {
            return new __InputValue
            {
                Name = arg.Key,
                Description = arg.Value.Meta?.Description,
                Type = BuildTypeRef(arg.Value.Type),
                DefaultValue = arg.Value.DefaultValue != null ? JsonConvert.SerializeObject(arg.Value.DefaultValue):null
            };
        }
    }
}