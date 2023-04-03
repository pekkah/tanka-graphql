using System.Threading.Tasks;

namespace Tanka.GraphQL.Server;

public delegate Task GraphQLRequestDelegate(GraphQLRequestContext context);