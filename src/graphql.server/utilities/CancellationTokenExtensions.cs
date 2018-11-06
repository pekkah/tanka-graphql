using System.Threading;
using System.Threading.Tasks;

namespace fugu.graphql.server.utilities
{
    internal static class CancellationTokenExtensions
    {
        /// <summary>
        ///     Allows awaiting cancellationToken.
        ///     todo: task might not complete ever in some situations?
        ///     See: https://github.com/dotnet/corefx/issues/2704#issuecomment-388776983
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WhenCancelled(this CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => { taskCompletionSource.TrySetResult(true); }))
            {
                await taskCompletionSource.Task;
            }
        }
    }
}