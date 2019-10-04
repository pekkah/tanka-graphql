using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Server
{
    public interface IQueryStreamService
    {
        Task<QueryStream> QueryAsync(
            Query query,
            CancellationToken cancellationToken);
    }
}