using System;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class ConnectionBuilder
    {
        public void EnsureTypeKnown(IType type)
        {
            var unwrappedType = type.Unwrap();

            if (unwrappedType == null)
                throw new SchemaBuilderException(
                    string.Empty,
                    $"Unwrapping type {type} not possible");

            if (!Builder.TryGetType<INamedType>(unwrappedType.Name, out _))
            {
                throw new SchemaBuilderException(
                    unwrappedType.Name,
                    $"Type {unwrappedType} is not known by the builder.");
            }
        }

        public void EnsureDirectiveKnown(DirectiveInstance instance)
        {
            EnsureDirectiveKnown(instance.Type);
        }

        public void EnsureDirectiveKnown(DirectiveType type)
        {
            if (!Builder.TryGetDirective(type.Name, out _))
            {
                throw new SchemaBuilderException(
                    type.Name,
                    $"Directive {type} is not known by the builder.");
            }
        }
    }
}