using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public interface ITypeSystemConfiguration
{
    Task Configure(TypeSystem.SchemaBuilder schema, ResolversBuilder resolvers);
}