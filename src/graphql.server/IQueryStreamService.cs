using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.DTOs;

namespace Tanka.GraphQL.Server
{
    public interface IQueryStreamService
    {
        Task<QueryStream> QueryAsync(
            Query query,
            CancellationToken cancellationToken);
    }
}