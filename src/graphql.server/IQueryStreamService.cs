using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.requests;

namespace tanka.graphql.server
{
    public interface IQueryStreamService
    {
        Task<QueryStream> QueryAsync(
            Query query,
            CancellationToken cancellationToken);
    }
}