using System.Linq;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SchemaBuilding
{
    public partial class SchemaBuilder
    {
        public SchemaBuilder Remove(INamedType type)
        {
            if (!TryGetType<INamedType>(type.Name, out _)) 
                return this;

            switch (type)
            {
                case ComplexType complexType:
                {
                    foreach (var field in _connections.GetFields(complexType).ToList())
                        _connections.Remove(complexType, field.Key);

                    break;
                }
                case InputObjectType inputObjectType:
                {
                    foreach (var field in _connections.GetInputFields(inputObjectType.Name).ToList())
                        _connections.Remove(inputObjectType, field.Key);

                    break;
                }
            }

            _types.Remove(type.Name);

            return this;
        }
    }
}