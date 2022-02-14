using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Server.Utils;

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
        if (!cancellationToken.CanBeCanceled)
            throw new InvalidOperationException(
                "WhenCancelled cannot be used on cancellationToken which can't be cancelled");

        var taskCompletionSource = new TaskCompletionSource<bool>();

        using (cancellationToken.Register(() => { taskCompletionSource.TrySetResult(true); }))
        {
            await taskCompletionSource.Task;
        }
    }
}