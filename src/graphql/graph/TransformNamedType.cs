using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.graph
{
    public class TransformNamedType : SchemaVisitorBase
    {
        private readonly Func<INamedType, INamedType> _transform;

        private readonly Dictionary<INamedType, INamedType> _transformedTypes =
            new Dictionary<INamedType, INamedType>();

        public TransformNamedType(Func<INamedType, INamedType> transform)
        {
            _transform = transform;
        }

        public ISchema Transform(ISchema schema)
        {
            var walker = new SchemaWalker(schema);
            walker.Visit(this);

            return Apply(schema);
        }

        private ISchema Apply(ISchema schema)
        {
            if (!_transformedTypes.Any())
                return schema;

            var removedTypes = new List<IType>();
            var newTypes = new List<IType>();
            var query = schema.Query;
            foreach (var transformedType in _transformedTypes)
            {
                var original = transformedType.Key;
                var transformed = transformedType.Value;

                if (original.Equals(schema.Query))
                    query = (ObjectType)transformed;

                if (transformed == null)
                    removedTypes.Add(original);

                newTypes.Add(transformed);;
            }

            return schema.WithQuery(query);
        }

        public override void VisitField(ObjectType objectType, KeyValuePair<string, IField> field)
        {
            var unwrappedFieldType = field.Value.Type.Unwrap();

            if (unwrappedFieldType is INamedType namedType)
                if (_transformedTypes.ContainsKey(namedType))
                    _transformedTypes[objectType] = objectType
                        .ExcludeFields(field)
                        .IncludeFields(field
                            .WithType(
                                WrapIfRequired(
                                    field.Value.Type, 
                                    _transformedTypes[namedType])));

            base.VisitField(objectType, field);
        }

        public override void VisitNamedType(INamedType namedType)
        {
            var maybeTransformedType = _transform(namedType);

            if (Equals(maybeTransformedType, namedType))
                return;

            _transformedTypes.Add(namedType, maybeTransformedType);
        }

        private IType WrapIfRequired(IType currentType, IType namedType)
        {
            if (currentType is NonNull nonNull)
                return new NonNull(WrapIfRequired(nonNull.WrappedType, namedType));

            if (currentType is List list)
                return new List(WrapIfRequired(list.WrappedType, namedType));

            return namedType;
        }
    }
}